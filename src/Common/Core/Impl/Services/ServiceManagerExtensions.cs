// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.Common.Core.Services {
    public static class ServiceManagerExtensions {
        public static IServiceManager AddService<TService>(this IServiceManager services) 
            where TService : class
            => AddService<TService, TService>(services);

        public static IServiceManager AddService<TService, TImplementation>(this IServiceManager services)
            where TService : class
            where TImplementation : class, TService
            => services.AddService<TService>(CreateInstance<TImplementation>);

        private static T CreateInstance<T>(IServiceContainer s) where T : class {
            return InstanceFactory<T>.New(s);
        }

        private static class InstanceFactory<T> where T : class {
            private static readonly Func<IServiceContainer, T> _factory;

            static InstanceFactory() {
                _factory = CreateFactory() 
                    ?? throw new InvalidOperationException($"Type {typeof(T)} should have either default constructor or constructor that accepts IServiceContainer");
            }

            public static T New(IServiceContainer services) => _factory(services);

            private static Func<IServiceContainer, T> CreateFactory() {
                var constructors = typeof(T).GetTypeInfo().DeclaredConstructors
                    .Where(c => c.IsPublic)
                    .ToList();

                foreach (var constructor in constructors) {
                    var parameters = constructor.GetParameters();
                    if (parameters.Length == 1 && typeof(IServiceContainer) == parameters[0].ParameterType) {
                        var parameter = Expression.Parameter(typeof(IServiceContainer), "services");
                        return Expression
                            .Lambda<Func<IServiceContainer, T>>(Expression.New(constructor, parameter), parameter)
                            .Compile();
                    }
                }

                foreach (var constructor in constructors) {
                    if (constructor.GetParameters().Length == 0) {
                        var parameter = Expression.Parameter(typeof(IServiceContainer), "services");
                        return Expression
                            .Lambda<Func<IServiceContainer, T>>(Expression.New(constructor), parameter)
                            .Compile();
                    }
                }

                return null;
            }
        }
    }
}