using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Common.Core;
using static System.FormattableString;

namespace Microsoft.UnitTests.Core.XUnit {
    public static class MethodFixtureProvider {
        private static readonly ConcurrentDictionary<Type, FixtureDescriptor> _fixtureDescriptors = new ConcurrentDictionary<Type, FixtureDescriptor>();

        public static IDictionary<int, object> CreateMethodFixtures(IReadOnlyDictionary<int, Type> methodFixtureTypes, IReadOnlyDictionary<Type, object> assemblyFixtures) {
            var fixtures = new Dictionary<int, object>();
            if (methodFixtureTypes.Count == 0) {
                return fixtures;
            }

            UpdateFixtureDescriptors(methodFixtureTypes.Values, assemblyFixtures);
            return CreateInstances(methodFixtureTypes, assemblyFixtures);
        }

        public static IEnumerable<MethodInfo> GetFactoryMethods(Type methodFixtureFactoryType) => methodFixtureFactoryType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(mi => mi.Name == "Create");

        private static void UpdateFixtureDescriptors(IEnumerable<Type> types, IReadOnlyDictionary<Type, object> assemblyFixtures) {
            var typesQueue = new Queue<Type>(types);
            while (typesQueue.Count > 0) {
                var type = typesQueue.Dequeue();

                var descriptor = _fixtureDescriptors.GetOrAdd(type, Create);

                foreach (var dependencyType in descriptor.ArgumentTypes.Where(t => !_fixtureDescriptors.ContainsKey(t))) {
                    typesQueue.Enqueue(dependencyType);
                }
            }

            FixtureDescriptor Create(Type type) => assemblyFixtures.TryGetValue(type, out var factory)
                ? CreateFactoryDescriptor(type, factory.GetType())
                : CreateInstanceDescriptor(type);
        }

        private static IDictionary<int, object> CreateInstances(IReadOnlyDictionary<int, Type> methodFixtureTypes, IReadOnlyDictionary<Type, object> assemblyFixtures) {
            var fixtures = new Dictionary<Type, object>();
            var types = new Stack<Type>(methodFixtureTypes.Values);
            while (types.Count > 0) {
                var type = types.Peek();
                if (fixtures.ContainsKey(type)) {
                    types.Pop();
                    continue;
                }

                var descriptor = _fixtureDescriptors[type];

                var dependencies = descriptor.ArgumentTypes.Where(t => !fixtures.ContainsKey(t)).ToList();
                if (dependencies.Count == 0) {
                    var args = descriptor.ArgumentTypes.Select(ft => fixtures[ft]).ToArray();
                    fixtures[type] = assemblyFixtures.TryGetValue(type, out var factory) 
                        ? descriptor.FactoryMethod((IMethodFixtureFactory)factory, args)
                        : Activator.CreateInstance(descriptor.Type, args);
                    types.Pop();
                } else {
                    foreach (var dependency in dependencies) {
                        types.Push(dependency);
                    }
                }
            }

            var fixturesByIndex = new Dictionary<int, object>();
            foreach (var (index, type) in methodFixtureTypes) {
                fixturesByIndex[index] = fixtures[type];
            }
            return fixturesByIndex;
        }

        private static FixtureDescriptor CreateInstanceDescriptor(Type type) {
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
            return new FixtureDescriptor(type, null, argumentTypes);
        }

        private static FixtureDescriptor CreateFactoryDescriptor(Type type, Type factoryType) {
            var createMethods = GetFactoryMethods(factoryType)
                .Where(mi => mi.ReturnType == type)
                .ToArray();

            if (createMethods.Length == 0) {
                throw new InvalidOperationException(Invariant($"{factoryType} has no \"Create\" methods"));
            }

            if (createMethods.Length > 1) {
                throw new InvalidOperationException(Invariant($"{factoryType} has more than one \"Create\" method"));
            }

            var createMethodInfo = createMethods[0];
            var createMethodParameters = createMethodInfo.GetParameters();
            var factoryMethod = CompileFactory(factoryType, createMethodInfo, createMethodParameters);
            var argumentTypes = createMethodParameters.Select(pi => pi.ParameterType).ToArray();
            return new FixtureDescriptor(factoryType, factoryMethod, argumentTypes);
        }

        private static Func<IMethodFixtureFactory, object[], object> CompileFactory(Type factoryType, MethodInfo createMethodInfo, ParameterInfo[] createMethodParameters) {
            var factoryInstanceParameter = Expression.Parameter(typeof(IMethodFixtureFactory), "f");
            var callInstance = Expression.Convert(factoryInstanceParameter, factoryType);
            var argsParameter = Expression.Parameter(typeof(object[]), "args");
            var callArguments = createMethodParameters
                .Select((pi, i) => Expression.Convert(Expression.ArrayIndex(argsParameter, Expression.Constant(i)), pi.ParameterType))
                .ToList();

            var call = Expression.Call(callInstance, createMethodInfo, callArguments);
            var lambda = Expression.Lambda<Func<IMethodFixtureFactory, object[], object>>(call, factoryInstanceParameter, argsParameter);
            return lambda.Compile();
        }

        private class FixtureDescriptor {
            public Func<IMethodFixtureFactory, object[], object> FactoryMethod { get; }
            public Type Type { get; }
            public Type[] ArgumentTypes { get; }

            public FixtureDescriptor(Type type, Func<IMethodFixtureFactory, object[], object> factoryMethod, Type[] argumentTypes) {
                Type = type;
                FactoryMethod = factoryMethod;
                ArgumentTypes = argumentTypes;
            }
        }
    }
}