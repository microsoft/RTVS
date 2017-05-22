using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static System.FormattableString;

namespace Microsoft.UnitTests.Core.XUnit {
    public static class MethodFixtureTypes {
        private static readonly ConcurrentDictionary<Type, FixtureType> _fixtureTypes = new ConcurrentDictionary<Type, FixtureType>();

        public static IDictionary<Type, object> CreateMethodFixtures(IList<Type> instanceTypes, IList<IMethodFixtureFactory<object>> factories) {
            var fixtures = new Dictionary<Type, object>();
            if (instanceTypes.Count == 0) {
                return fixtures;
            }

            UpdateFixtureTypes(instanceTypes, factories);
            CreateMethodFixtures(instanceTypes, factories, fixtures);
            return fixtures;
        }

        private static void UpdateFixtureTypes(IEnumerable<Type> types, IList<IMethodFixtureFactory<object>> factories) {
            var typesQueue = new Queue<Type>(types);
            while (typesQueue.Count > 0) {
                var type = typesQueue.Dequeue();

                var fixtureType = _fixtureTypes.GetOrAdd(type, t => {
                    var factory = GetFactory(factories, type);
                    return factory != null ? CreateForFactory(factory.GetType()) : CreateForInstance(type);
                });

                foreach (var dependencyType in fixtureType.ArgumentTypes) {
                    typesQueue.Enqueue(dependencyType);
                }
            }
        }

        private static void CreateMethodFixtures(IEnumerable<Type> instanceTypes, IList<IMethodFixtureFactory<object>> factories, IDictionary<Type, object> fixtures) {
            foreach (var type in instanceTypes.Where(t => !fixtures.ContainsKey(t))) {
                var existingType = fixtures.Keys.FirstOrDefault(ft => type.IsAssignableFrom(ft));
                if (existingType != null) {
                    fixtures[type] = fixtures[existingType];
                    continue;
                }

                var fixtureType = _fixtureTypes[type];
                var factory = GetFactory(factories, type);
                if (fixtureType.ArgumentTypes.Any()) {
                    CreateMethodFixtures(fixtureType.ArgumentTypes, factories, fixtures);
                }

                fixtures[type] = fixtureType.CreateInstance(factory, fixtures);
            }
        }
        
        private static IMethodFixtureFactory<object> GetFactory(IList<IMethodFixtureFactory<object>> factories, Type type) 
            => factories.FirstOrDefault(f => type.IsInstanceOfType(f.Dummy));


        private static FixtureType CreateForInstance(Type type) {
            var constructors = type.GetConstructors();
            if (constructors.Length == 0) {
                throw new InvalidOperationException(Invariant($"{type} has no constructors"));
            }

            // Default constructor is required for dummy object
            // If there are 2 constructors, use the one with parameters to construct real object
            var defaultConstructorIndex = -1;
            if (constructors.Length == 2) {
                if (constructors[0].GetParameters().Length == 0) {
                    defaultConstructorIndex = 0;
                } else if (constructors[1].GetParameters().Length == 0) {
                    defaultConstructorIndex = 1;
                }
            }

            if (constructors.Length > 1 && defaultConstructorIndex == -1) {
                throw new InvalidOperationException(Invariant($"{type} has more than one constructor with parameters"));
            }

            var constructor = constructors.Length == 2 ? constructors[1 - defaultConstructorIndex] : constructors[0];
            var argumentTypes = constructor.GetParameters().Select(pi => pi.ParameterType).ToArray();
            return new FixtureType(type, null, argumentTypes);
        }

        private static FixtureType CreateForFactory(Type type) {
            var createMethods = type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(mi => mi.Name == "Create" && typeof(IMethodFixture).IsAssignableFrom(mi.ReturnType))
                .ToArray();

            if (createMethods.Length == 0) {
                throw new InvalidOperationException(Invariant($"{type} has no \"Create\" methods"));
            }

            if (createMethods.Length != 1) {
                throw new InvalidOperationException(Invariant($"{type} has more than one \"Create\" method"));
            }

            var createMethodInfo = createMethods[0];
            var createMethodParameters = createMethodInfo.GetParameters();
            var factoryMethod = CompileFactory(type, createMethodInfo, createMethodParameters);
            var argumentTypes = createMethodParameters.Select(pi => pi.ParameterType).ToArray();
            return new FixtureType(type, factoryMethod, argumentTypes);
        }

        private static Func<IMethodFixtureFactory<object>, object[], object> CompileFactory(Type factoryType, MethodInfo createMethodInfo, ParameterInfo[] createMethodParameters) {
            var factoryInstanceParameter = Expression.Parameter(typeof(IMethodFixtureFactory<object>), "f");
            var callInstance = Expression.Convert(factoryInstanceParameter, factoryType);
            var argsParameter = Expression.Parameter(typeof(object[]), "args");
            var callArguments = createMethodParameters
                .Select((pi, i) => Expression.Convert(Expression.ArrayIndex(argsParameter, Expression.Constant(i)), pi.ParameterType))
                .ToList();

            var call = Expression.Call(callInstance, createMethodInfo, callArguments);
            var lambda = Expression.Lambda<Func<IMethodFixtureFactory<object>, object[], IMethodFixture>>(call, factoryInstanceParameter, argsParameter);
            return lambda.Compile();
        }

        private class FixtureType {
            private readonly Func<IMethodFixtureFactory<object>, object[], object> _factoryMethod;
            private readonly Type _type;
            public Type[] ArgumentTypes { get; }

            public FixtureType(Type type, Func<IMethodFixtureFactory<object>, object[], object> factoryMethod, Type[] argumentTypes) {
                _type = type;
                _factoryMethod = factoryMethod;
                ArgumentTypes = argumentTypes;
            }

            public object CreateInstance(IMethodFixtureFactory<object> factory, IDictionary<Type, object> fixtures) {
                var args = ArgumentTypes.Select(ft => fixtures[ft]).ToArray();
                return _factoryMethod != null ? _factoryMethod(factory, args) : Activator.CreateInstance(_type, args);
            }
        }
    }
}