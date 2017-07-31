// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Disposables;
using static System.FormattableString;

namespace Microsoft.Common.Core.Services {
    public class ServiceManager : IServiceManager {
        private readonly DisposeToken _disposeToken = DisposeToken.Create<ServiceManager>();
        private readonly ConcurrentDictionary<Type, object> _s = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Add service to the service manager container
        /// </summary>
        /// <param name="service">Service instance</param>
        /// <param name="type">
        /// Optional type to register the instance for. In Visual Studio
        /// some global services are registered as 'SVsService` while
        /// actual interface type is IVsService.
        /// </param>
        public virtual IServiceManager AddService(object service, Type type = null) {
            _disposeToken.ThrowIfDisposed();

            type = type ?? service.GetType();
            Check.ArgumentNull(nameof(service), service);
            Check.InvalidOperation(() => _s.GetOrAdd(type, service) == service, 
                Invariant($"Another instance of service of type {type} already added"));
            return this;
        }

        /// <summary>
        /// Adds on-demand created service
        /// </summary>
        public virtual IServiceManager AddService<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
            => AddLazyService(typeof(TService), typeof(TImplementation), null);

        /// <summary>
        /// Adds on-demand created service
        /// </summary>
        /// <param name="factory">Service factory</param>
        public IServiceManager AddService<T>(Func<IServiceManager, T> factory) where T : class
            => AddLazyService(typeof(T), typeof(T), s => factory(this));

        private IServiceManager AddLazyService(Type serviceType, Type implementationType, Func<IServiceManager, object> factory) {
            _disposeToken.ThrowIfDisposed();

            var lazy = new LazyService(implementationType, this, factory);
            Check.InvalidOperation(() => _s.TryAdd(serviceType, lazy), $"Service of type {serviceType} already exists");
            return this;
        }

        /// <summary>
        /// Retrieves service from the container
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>Service instance or null if it doesn't exist</returns>
        public virtual T GetService<T>(Type type = null) where T : class {
            if (_disposeToken.IsDisposed) {
                // Do not throw. When editor text buffer is closed, the associated service manager
                // is disposed. However, some actions may still hold on the text buffer reference
                // and actually determine if buffer is closed by checking if editor document 
                // is still attached as a service.
                return null;
            }

            type = type ?? typeof(T);
            if (!_s.TryGetValue(type, out object value)) {
                value = _s.FirstOrDefault(kvp => type.GetTypeInfo().IsAssignableFrom(kvp.Key)).Value;
            }

            if (value is T) {
                return (T)CheckDisposed(value);
            }

            if (value is LazyService ls) {
                return (T)CheckDisposed(ls.Instance);
            }

            var ti = type.GetTypeInfo();
            value = _s.FirstOrDefault(kvp => {
                var lzs = kvp.Value as LazyService;
                return lzs != null && ti.IsAssignableFrom(lzs.ImplementationType);
            }).Value;

            return (T)CheckDisposed((value as LazyService)?.Instance);
        }

        public virtual void RemoveService(object service) => RemoveService(service.GetType());
        public virtual void RemoveService<T>() => RemoveService(typeof(T));

        private void RemoveService(Type type) {
            if (_s.TryRemove(type, out object dummy)) {
                return;
            }

            var ti = type.GetTypeInfo();
            var implementor = _s.FirstOrDefault(kvp => ti.IsAssignableFrom(kvp.Key));
            if (implementor.Key != null && _s.TryRemove(implementor.Key, out dummy)) {
                return;
            }

            var lazy = _s.FirstOrDefault(kvp => {
                var lzs = kvp.Value as LazyService;
                return lzs != null && ti.IsAssignableFrom(lzs.ImplementationType);
            });

            if (lazy.Key != null) {
                _s.TryRemove(lazy.Key, out dummy);
            }
        }

        public virtual IEnumerable<Type> AllServices => _s.Keys.ToList();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object CheckDisposed(object service) {
            if (_disposeToken.IsDisposed) {
                (service as IDisposable)?.Dispose();
                _disposeToken.ThrowIfDisposed();
            }
            return service;
        }

        #region IDisposable

        public void Dispose() {
            if (_disposeToken.TryMarkDisposed()) {
                foreach (var service in _s.Values) {
                    if (service is LazyService lzs && lzs.IsInstanceCreated) {
                        (lzs.Instance as IDisposable)?.Dispose();
                    } else {
                        (service as IDisposable)?.Dispose();
                    }
                }
            }
        }

        #endregion

        private class LazyService {
            private readonly IServiceManager _sm;
            private readonly Lazy<object> _instance;

            public Type ImplementationType { get; }
            public object Instance => _instance.Value;
            public bool IsInstanceCreated => _instance.IsValueCreated;

            public LazyService(Type implementationType, IServiceManager sm, Func<IServiceManager, object> factory) {
                ImplementationType = implementationType;
                _sm = sm;
                _instance = factory != null
                                ? new Lazy<object>(() => factory(_sm))
                                : new Lazy<object>(CreateFactory);
            }

            private object CreateFactory() {
                try {
                    var constructors = ImplementationType.GetTypeInfo().DeclaredConstructors
                        .Where(c => c.IsPublic)
                        .ToList();

                    foreach (var constructor in constructors) {
                        var parameters = constructor.GetParameters();
                        if (parameters.Length == 1 &&
                               (typeof(IServiceContainer) == parameters[0].ParameterType ||
                                typeof(IServiceManager) == parameters[0].ParameterType)) {
                            return constructor.Invoke(new object[] { _sm });
                        }
                    }

                    foreach (var constructor in constructors) {
                        if (constructor.GetParameters().Length == 0) {
                            return constructor.Invoke(new object[0]);
                        }
                    }
                } catch (TargetInvocationException) { }

                throw new InvalidOperationException($"Type {ImplementationType} should have either default constructor or constructor that accepts IServiceContainer");
            }
        }
    }
}