// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.Services {
    public abstract class ServiceManagerBase : IDisposable {
        private IPropertyOwner _propertyOwner;
        private readonly object _lock = new object();
        private readonly Dictionary<Type, object> _servicesByType = new Dictionary<Type, object>();

        /// <summary>
        /// Fire when service is added
        /// </summary>
        public event EventHandler<ServiceManagerEventArgs> ServiceAdded;
        /// <summary>
        /// Fires when service is removed
        /// </summary>
        public event EventHandler<ServiceManagerEventArgs> ServiceRemoved;

        protected static ServiceManagerBase FromPropertyOwner(IPropertyOwner propertyOwner, Func<IPropertyOwner, ServiceManagerBase> factory) {
            ServiceManagerBase sm = null;

            if (propertyOwner.Properties.ContainsProperty(typeof (ServiceManagerBase))) {
                sm = propertyOwner.Properties.GetProperty(typeof (ServiceManagerBase)) as ServiceManagerBase;
            } else if (factory != null) {
                sm = factory(propertyOwner);
            }

            return sm;
        }

        /// <summary>
        /// Retrieves service from a service manager for this Property owner given service type
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="propertyOwner">Property owner</param>
        /// <returns>Service instance</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static T GetService<T>(IPropertyOwner propertyOwner) where T : class {
            try {
                var sm = FromPropertyOwner(propertyOwner, null);
                return sm?.GetService<T>();
            } catch (Exception) {
                return null;
            }
        }

        public static ICollection<T> GetAllServices<T>(IPropertyOwner propertyOwner) where T : class {
            var sm = FromPropertyOwner(propertyOwner, null);
            return sm != null ? sm.GetAllServices<T>() : new List<T>();
        }

        protected ServiceManagerBase(IPropertyOwner propertyOwner) {
            _propertyOwner = propertyOwner;
            _propertyOwner.Properties.AddProperty(typeof(ServiceManagerBase), this);
        }

        protected T GetService<T>() where T : class {
            return GetService<T>(true);
        }

        protected T GetService<T>(bool checkDerivation) where T : class {
            lock (_lock) {
                object service;

                if (_servicesByType.TryGetValue(typeof (T), out service) || !checkDerivation) {
                    return service as T;
                }

                // try walk through and cast. Perhaps someone is asking for IFoo
                // that is implemented on class Bar but Bar was added as Bar, not as IFoo
                foreach (var kvp in _servicesByType) {
                    service = kvp.Value as T;
                    if (service != null)
                        break;
                }

                return (T) service;
            }
        }

        private ICollection<T> GetAllServices<T>() where T : class {
            lock (_lock) {
                return _servicesByType.Values.OfType<T>().ToList();
            }
        }

        protected void AddService<T>(T serviceInstance) where T : class {
            var added = false;

            lock (_lock) {
                if (GetService<T>(false) == null) {
                    _servicesByType.Add(typeof(T), serviceInstance);
                    added = true;
                }
            }

            if (added) {
                ServiceAdded?.Invoke(this, new ServiceManagerEventArgs(typeof(T), serviceInstance));
            }
        }

        protected void RemoveService<T>() where T : class {
            bool foundServiceInstance;
            object serviceInstance;

            lock (_lock) {
                foundServiceInstance = _servicesByType.TryGetValue(typeof(T), out serviceInstance);

                if (foundServiceInstance) {
                    _servicesByType.Remove(typeof(T));
                }
            }

            if (foundServiceInstance) {
                ServiceRemoved?.Invoke(this, new ServiceManagerEventArgs(typeof(T), serviceInstance));
            }
        }

        public void Dispose() {
            if (_propertyOwner == null) {
                return;
            }

            _propertyOwner.Properties.RemoveProperty(typeof(ServiceManagerBase));
            foreach (var d in _servicesByType.Values.OfType<IDisposable>()) {
                d.Dispose();
            }

            _servicesByType.Clear();
            _propertyOwner = null;
        }
    }
}