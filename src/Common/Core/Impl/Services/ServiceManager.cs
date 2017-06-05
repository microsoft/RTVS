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

namespace Microsoft.Common.Core.Services {
    public class ServiceManager : IServiceManager {
        private readonly DisposeToken _disposeToken = DisposeToken.Create<ServiceManager>();
        private readonly ConcurrentDictionary<Type, object> _s = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Add service to the service manager container
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="service">Service instance</param>
        /// <param name="type">
        /// Optional type to register the instance for. In Visual Studio
        /// some global services are registered as 'SVsService` while
        /// actual interface type is IVsService.
        /// </param>
        public virtual IServiceManager AddService<T>(T service, Type type = null) where T : class {
            _disposeToken.ThrowIfDisposed();

            type = type ?? typeof(T);
            Check.ArgumentNull(nameof(service), service);
            Check.InvalidOperation(() => _s.TryAdd(type, service), $"Service of type {type} already exists");

            return this;
        }

        /// <summary>
        /// Adds on-demand created service
        /// </summary>
        /// <param name="factory">Service factory</param>
        public virtual IServiceManager AddService<T>(Func<IServiceManager, T> factory) where T : class {
            _disposeToken.ThrowIfDisposed();

            var lazy = new Lazy<object>(() => factory(this));
            Check.InvalidOperation(() => _s.TryAdd(typeof(T), lazy), $"Service of type {typeof(T)} already exists");
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

            return (T)CheckDisposed(value as T ?? (value as Lazy<object>)?.Value);
        }

        public virtual void RemoveService(object service) => _s.TryRemove(service.GetType(), out object dummy);

        public virtual void RemoveService<T>() => _s.TryRemove(typeof(T), out object dummy);

        public virtual IEnumerable<Type> AllServices => _s.Keys.ToList();

        public virtual IEnumerable<T> GetServices<T>() where T : class {
            if (_disposeToken.IsDisposed) {
                yield break;
            }

            var type = typeof(T);
            foreach (var value in _s.Values.OfType<T>()) {
                CheckDisposed(value);
                yield return value;
            }

            // Perhaps someone is asking for IFoo that is implemented on class Bar 
            // but Bar was added as Bar, not as IFoo
            foreach (var kvp in _s.Where(kvp => kvp.Value is Lazy<object> && type.GetTypeInfo().IsAssignableFrom(kvp.Key))) {
                yield return (T)CheckDisposed(((Lazy<object>)kvp.Value).Value);
            }
        }

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
                    if (service is Lazy<object> lazy && lazy.IsValueCreated) {
                        (lazy.Value as IDisposable)?.Dispose();
                    } else {
                        (service as IDisposable)?.Dispose();
                    }
                }
            }
        }
        #endregion
    }
}