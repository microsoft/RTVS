// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Extensions;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Services {
    public sealed class ServiceManager : IDisposable {
        private readonly object _lock = new object();
        private readonly ICoreShell _shell;
        private readonly Dictionary<Type, object> _servicesByType = new Dictionary<Type, object>();
        private IPropertyOwner _propertyOwner;

        /// <summary>
        /// Fire when service is added
        /// </summary>
        public event EventHandler<ServiceManagerEventArgs> ServiceAdded;
        /// <summary>
        /// Fires when service is removed
        /// </summary>
        public event EventHandler<ServiceManagerEventArgs> ServiceRemoved;

        public static void AdviseServiceAdded<T>(IPropertyOwner propertyOwner, ICoreShell shell, Action<T> callback) where T : class {
            var sm = FromPropertyOwner(propertyOwner, shell);

            var existingService = sm.GetService<T>();
            if (existingService != null) {
                callback(existingService);
            } else {
                EventHandler<ServiceManagerEventArgs> onServiceAdded = null;
                onServiceAdded = (sender, eventArgs) => {
                    if (eventArgs.ServiceType == typeof (T)) {
                        callback(eventArgs.Service as T);
                        sm.ServiceAdded -= onServiceAdded;
                    }
                };

                sm.ServiceAdded += onServiceAdded;
            }
        }

        public static void AdviseServiceRemoved<T>(IPropertyOwner propertyOwner, ICoreShell shell, Action<T> callback) where T : class {
            var sm = FromPropertyOwner(propertyOwner, shell);

            EventHandler<ServiceManagerEventArgs> onServiceRemoved = null;
            onServiceRemoved = (sender, eventArgs) => {
                if (eventArgs.ServiceType == typeof (T)) {
                    callback(eventArgs.Service as T);
                    sm.ServiceRemoved -= onServiceRemoved;
                }
            };

            sm.ServiceRemoved += onServiceRemoved;
        }

        private ServiceManager(IPropertyOwner propertyOwner, ICoreShell coreShell) {
            _propertyOwner = propertyOwner;
            _propertyOwner.Properties.AddProperty(typeof(ServiceManager), this);
            _shell = coreShell;

            var textView = propertyOwner as ITextView;
            if (textView != null) {
                textView.Closed += TextViewClosed;
            } else if (propertyOwner is ITextBuffer) {
                var textBuffer = (ITextBuffer) propertyOwner;

                // Need to wait to idle as the TextViewConnectListener.OnTextBufferDisposing hasn't fired yet.
                textBuffer.AddBufferDisposedAction(_shell, DisposeServiceManagerOnIdle);
            }
        }

        private static void DisposeServiceManagerOnIdle(IPropertyOwner propertyOwner, ICoreShell shell) {
            var sm = FromPropertyOwner(propertyOwner, null);
            if (sm != null) {
                IdleTimeAction.Create(() => sm.Dispose(), 150, new object(), shell);
            }
        }

        private void TextViewClosed(object sender, EventArgs e) {
            var textView = (ITextView) sender;
            textView.Closed -= TextViewClosed;

            // Need to wait to idle as taggers can also get disposed during TextView.Closed notifications
            DisposeServiceManagerOnIdle(textView, _shell);
        }

        public static ServiceManager TryGetOrCreate(IPropertyOwner propertyOwner, Func<IPropertyOwner, ServiceManager> factory) {
            ServiceManager sm = null;

            if (propertyOwner.Properties.ContainsProperty(typeof(ServiceManager))) {
                sm = propertyOwner.Properties.GetProperty(typeof(ServiceManager)) as ServiceManager;
            } else if (factory != null) {
                sm = factory(propertyOwner);
            }

            return sm;
        }

        /// <summary>
        /// Returns service manager attached to a given Property owner
        /// </summary>
        /// <param name="propertyOwner">Property owner</param>
        /// <param name="shell"></param>
        /// <returns>Service manager instance</returns>
        public static ServiceManager FromPropertyOwner(IPropertyOwner propertyOwner, ICoreShell shell) {
            return TryGetOrCreate(propertyOwner, po => new ServiceManager(po, shell));
        }

        /// <summary>
        /// Add service to a service manager associated with a particular Property owner
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="serviceInstance">Service instance</param>
        /// <param name="propertyOwner">Property owner</param>
        /// <param name="shell"></param>
        public static void AddService<T>(T serviceInstance, IPropertyOwner propertyOwner, ICoreShell shell) where T : class {
            var sm = FromPropertyOwner(propertyOwner, shell);
            Debug.Assert(sm != null);

            sm.AddService(serviceInstance);
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
                var sm = TryGetOrCreate(propertyOwner, null);
                return sm?.GetService<T>();
            } catch (Exception) {
                return null;
            }
        }

        public static ICollection<T> GetAllServices<T>(IPropertyOwner propertyOwner) where T : class {
            var sm = TryGetOrCreate(propertyOwner, null);
            return sm != null ? sm.GetAllServices<T>() : new List<T>();
        }

        public static void RemoveService<T>(IPropertyOwner propertyOwner) where T : class {
            if (propertyOwner != null) {
                var sm = TryGetOrCreate(propertyOwner, null);
                sm?.RemoveService<T>();
            }
        }

        private void AddService<T>(T serviceInstance) where T : class {
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

        private void RemoveService<T>() where T : class {
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

        private ICollection<T> GetAllServices<T>() where T : class {
            lock (_lock) {
                return _servicesByType.Values.OfType<T>().ToList();
            }
        }

        private T GetService<T>() where T : class => GetService<T>(true);

        private T GetService<T>(bool checkDerivation) where T : class {
            lock (_lock) {
                object service;

                if (_servicesByType.TryGetValue(typeof(T), out service) || !checkDerivation) {
                    return service as T;
                }

                // try walk through and cast. Perhaps someone is asking for IFoo
                // that is implemented on class Bar but Bar was added as Bar, not as IFoo
                foreach (var kvp in _servicesByType) {
                    service = kvp.Value as T;
                    if (service != null)
                        break;
                }

                return (T)service;
            }
        }

        public void Dispose() {
            if (_propertyOwner == null) {
                return;
            }

            _propertyOwner.Properties.RemoveProperty(typeof(ServiceManager));
            foreach (var d in _servicesByType.Values.OfType<IDisposable>()) {
                d.Dispose();
            }

            _servicesByType.Clear();
            _propertyOwner = null;
        }
    }
}