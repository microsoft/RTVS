// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Disposables;

namespace Microsoft.Common.Core.Services {
    public class ServiceManager : IServiceManager {
        private readonly DisposeToken _disposeToken = DisposeToken.Create<ServiceManager>();
        private readonly ConcurrentDictionary<Type, object> _s = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Fire when service is added
        /// </summary>
        public event EventHandler<ServiceContainerEventArgs> ServiceAdded;
        /// <summary>
        /// Fires when service is removed
        /// </summary>
        public event EventHandler<ServiceContainerEventArgs> ServiceRemoved;

        /// <summary>
        /// Add service to the service manager container
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="service">Service instance</param>
        public virtual IServiceManager AddService<T>(T service) where T : class {
            _disposeToken.ThrowIfDisposed();

            var type = typeof(T);
            Check.ArgumentNull(nameof(service), service);
            Check.Operation(() => _s.TryAdd(type, service));
            ServiceAdded?.Invoke(this, new ServiceContainerEventArgs(type));
            return this;
        }

        /// <summary>
        /// Adds on-demand created service
        /// </summary>
        /// <param name="factory">Optional creator function. If not provided, reflection with default constructor will be used.</param>
        public virtual IServiceManager AddService<T>(Func<T> factory) {
            _disposeToken.ThrowIfDisposed();

            var type = typeof(T);
            var lazy = factory != null 
                ? new Lazy<object>(() => factory()) 
                : new Lazy<object>(() => Activator.CreateInstance(type));

            Check.Operation(() => _s.TryAdd(type, lazy));
            ServiceAdded?.Invoke(this, new ServiceContainerEventArgs(typeof(T)));
            return this;
        }

        /// <summary>
        /// Retrieves service from the container
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>Service instance or null if it doesn't exist</returns>
        public virtual T GetService<T>(Type type = null) where T : class {
            _disposeToken.ThrowIfDisposed();

            type = type ?? typeof(T);
            if (!_s.TryGetValue(type, out object value)) {
                value = _s.FirstOrDefault(kvp => type.GetTypeInfo().IsAssignableFrom(kvp.Key));
            }

            return (T)CheckDisposed(value as T ?? (value as Lazy<object>)?.Value);
        }

        public virtual void RemoveService<T>() where T : class {
            _disposeToken.ThrowIfDisposed();

            if (_s.TryRemove(typeof(T), out object dummy)) {
                ServiceRemoved?.Invoke(this, new ServiceContainerEventArgs(typeof(T)));
            }
        }

        public virtual IEnumerable<Type> Services {
            get {
                _disposeToken.ThrowIfDisposed();
                return _s.Keys.ToList();
            }
        }

        public virtual IEnumerable<T> GetServices<T>() where T : class {
            _disposeToken.ThrowIfDisposed();

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
                    var lazy = service as Lazy<object>;
                    if (lazy != null && lazy.IsValueCreated) {
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