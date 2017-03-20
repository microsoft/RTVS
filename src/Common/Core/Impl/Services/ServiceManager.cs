// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Common.Core.Diagnostics;

namespace Microsoft.Common.Core.Services {
    public class ServiceManager : IServiceManager {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Func<object>> _deferredServices = new Dictionary<Type, Func<object>>();
        private readonly object _lock = new object();

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
            var type = typeof(T);
            Check.ArgumentNull(nameof(service), service);
            lock (_lock) {
                Check.InvalidOperation(() => _services.ContainsKey(type));
                _services[type] = service;
            }
            ServiceAdded?.Invoke(this, new ServiceContainerEventArgs(type));
            return this;
        }

        /// <summary>
        /// Adds on-demand created service
        /// </summary>
        /// <param name="type">Service type</param>
        /// <param name="factory">Optional creator function. If not provided, reflection with default constructor will be used.</param>
        /// <param name="parameters">Factory parameters</param>
        public virtual IServiceManager AddService(Type type, Func<object> factory) {
            Check.ArgumentNull(nameof(type), type);
            lock (_lock) {
                Check.InvalidOperation(() => _services.ContainsKey(type));
                Check.InvalidOperation(() => _deferredServices.ContainsKey(type));
                _deferredServices[type] = factory;
            }

            ServiceAdded?.Invoke(this, new ServiceContainerEventArgs(type));
            return this;
        }

        /// <summary>
        /// Retrieves service from the container
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>Service instance or null if it doesn't exist</returns>
        public virtual T GetService<T>(Type type = null) where T : class {
            type = type ?? typeof(T);

            lock (_lock) {
                object service;
                if (_services.TryGetValue(type, out service)) {
                    return service as T;
                }

                // Try walk through and cast. Perhaps someone is asking for IFoo
                // that is implemented on class Bar but Bar was added as Bar, not as IFoo
                service = _services.Values.Where(s => s is T).FirstOrDefault();
                if (service != null) {
                    return service as T;
                }

                // Check deferred
                Func<object> factory;
                if (_deferredServices.TryGetValue(type, out factory)) {
                    return CreateService(type, factory) as T;
                }

                foreach (var key in _deferredServices.Keys) {
                    if (type.GetTypeInfo().IsAssignableFrom(key)) {
                        return CreateService(key, _deferredServices[key]) as T;
                    }
                }

                return null;
            }
        }

        private object CreateService(Type type, Func<object> factory) {
            var serviceInstance = factory != null ? factory() : Activator.CreateInstance(type, this);
            _services[type] = serviceInstance;
            _deferredServices.Remove(type);
            return serviceInstance;
        }

        public virtual void RemoveService<T>() where T : class {
            bool fireEvent = false;
            lock (_lock) {
                fireEvent = _services.Remove(typeof(T)) || _deferredServices.Remove(typeof(T));
            }

            if (fireEvent) {
                ServiceRemoved?.Invoke(this, new ServiceContainerEventArgs(typeof(T)));
            }
        }

        public virtual IEnumerable<Type> Services {
            get {
                lock (_lock) {
                    var list = _services.Keys.ToList();
                    list.AddRange(_deferredServices.Keys);
                    return list;
                }
            }
        }

        public virtual IEnumerable<T> GetServices<T>() where T : class {
            lock (_lock) {
                // Perhaps someone is asking for IFoo that is implemented on class Bar 
                // but Bar was added as Bar, not as IFoo
                foreach(var s in _services.Values.Where(s => s is T)) {
                    yield return s as T;
                }

                foreach (var key in _deferredServices.Keys) {
                    if (typeof(T).GetTypeInfo().IsAssignableFrom(key)) {
                        yield return CreateService(key, _deferredServices[key]) as T;
                    }
                }
            }
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing) {
            Dispose();
        }

        public void Dispose() {
            lock (_lock) {
                foreach (var d in _services.Values.OfType<IDisposable>().ToList()) {
                    d.Dispose();
                }
                _services.Clear();
                _deferredServices.Clear();
            }
        }
        #endregion
    }
}