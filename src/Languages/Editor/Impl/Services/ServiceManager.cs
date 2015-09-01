using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Languages.Editor.EditorHelpers;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Services
{
    public sealed class ServiceManager : IDisposable
    {
        private IPropertyOwner _propertyOwner;
        private object _lock = new object();

        /// <summary>
        /// Fire when service is added
        /// </summary>
        public event EventHandler<ServiceManagerEventArgs> ServiceAdded;
        /// <summary>
        /// Fires when service is removed
        /// </summary>
        public event EventHandler<ServiceManagerEventArgs> ServiceRemoved;

        private Dictionary<Type, object> _servicesByType = new Dictionary<Type, object>();
        private Dictionary<Tuple<Type, string>, object> _servicesByContentType = new Dictionary<Tuple<Type, string>, object>();

        public static void AdviseServiceAdded<T>(IPropertyOwner propertyOwner, Action<T> callback) where T : class
        {
            ServiceManager sm = ServiceManager.FromPropertyOwner(propertyOwner, true);

            T existingService = sm.GetService<T>();
            if (existingService != null)
            {
                callback(existingService);
            }
            else
            {
                EventHandler<ServiceManagerEventArgs> onServiceAdded = null;
                onServiceAdded = (object sender, ServiceManagerEventArgs eventArgs) =>
                {
                    if (eventArgs.ServiceType == typeof(T))
                    {
                        callback(eventArgs.Service as T);
                        sm.ServiceAdded -= onServiceAdded;
                    }
                };

                sm.ServiceAdded += onServiceAdded;
            }
        }

        public static void AdviseServiceRemoved<T>(IPropertyOwner propertyOwner, Action<T> callback) where T : class
        {
            ServiceManager sm = ServiceManager.FromPropertyOwner(propertyOwner, true);

            EventHandler<ServiceManagerEventArgs> onServiceRemoved = null;
            onServiceRemoved = (object sender, ServiceManagerEventArgs eventArgs) =>
            {
                if (eventArgs.ServiceType == typeof(T))
                {
                    callback(eventArgs.Service as T);
                    sm.ServiceRemoved -= onServiceRemoved;
                }
            };

            sm.ServiceRemoved += onServiceRemoved;
        }

        private ServiceManager(IPropertyOwner propertyOwner)
        {
            _propertyOwner = propertyOwner;
            _propertyOwner.Properties.AddProperty(typeof(ServiceManager), this);

            if (propertyOwner is ITextView)
            {
                ITextView textView = (ITextView)propertyOwner;
                textView.Closed += OnViewClosed;
            }
            else if (propertyOwner is ITextBuffer)
            {
                ITextBuffer textBuffer = (ITextBuffer)propertyOwner;

                // Need to wait to idle as the TextViewConnectListener.OnTextBufferDisposing hasn't fired yet.
                textBuffer.AddBufferDisposedAction(DisposeServiceManagerOnIdle);
            }
        }

        private void DisposeServiceManagerOnIdle(IPropertyOwner propertyOwner)
        {
            ServiceManager sm = ServiceManager.FromPropertyOwner(propertyOwner, false);
            if (sm != null)
            {
                IdleTimeAction.Create(() =>
                {
                    sm.Dispose();
                }, 150, new object());
            }
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            ITextView textView = (ITextView)sender;
            textView.Closed -= OnViewClosed;

            // Need to wait to idle as taggers can also get disposed during TextView.Closed notifications
            DisposeServiceManagerOnIdle(textView);
        }

        /// <summary>
        /// Returns service manager attached to a given Property owner
        /// </summary>
        /// <param name="propertyOwner">Property owner</param>
        /// <returns>Service manager instance</returns>
        public static ServiceManager FromPropertyOwner(IPropertyOwner propertyOwner)
        {
            return FromPropertyOwner(propertyOwner, true);
        }

        public static ServiceManager FromPropertyOwner(IPropertyOwner propertyOwner, bool ensureCreated)
        {
            ServiceManager sm = null;

            if (propertyOwner.Properties.ContainsProperty(typeof(ServiceManager)))
                sm = propertyOwner.Properties.GetProperty(typeof(ServiceManager)) as ServiceManager;
            else if (ensureCreated)
                sm = new ServiceManager(propertyOwner);

            return sm;
        }

        /// <summary>
        /// Retrieves service from a service manager for this Property owner given service type
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="propertyOwner">Property owner</param>
        /// <returns>Service instance</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static T GetService<T>(IPropertyOwner propertyOwner) where T : class
        {
            try
            {
                var sm = ServiceManager.FromPropertyOwner(propertyOwner);
                Debug.Assert(sm != null);

                return sm.GetService<T>();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        ///  Retrieves service from a service manager for this Property owner given service type and content type
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="propertyOwner">Property owner</param>
        /// <param name="contentType">Content type</param>
        /// <returns>Service instance</returns>
        public static T GetService<T>(IPropertyOwner propertyOwner, IContentType contentType) where T : class
        {
            var sm = ServiceManager.FromPropertyOwner(propertyOwner);
            if (sm != null)
                return sm.GetService<T>(contentType);

            return null;
        }

        public static ICollection<T> GetAllServices<T>(IPropertyOwner propertyOwner) where T : class
        {
            var sm = ServiceManager.FromPropertyOwner(propertyOwner);
            if (sm != null)
                return sm.GetAllServices<T>();

            return new List<T>();
        }

        /// <summary>
        /// Add service to a service manager associated with a particular Property owner
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="serviceInstance">Service instance</param>
        /// <param name="propertyOwner">Property owner</param>
        public static void AddService<T>(T serviceInstance, IPropertyOwner propertyOwner) where T : class
        {
            var sm = ServiceManager.FromPropertyOwner(propertyOwner);
            Debug.Assert(sm != null);

            sm.AddService<T>(serviceInstance);
        }

        /// <summary>
        /// Add content type specific service to a service manager associated with a particular Property owner
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="serviceInstance">Service instance</param>
        /// <param name="propertyOwner">Property owner</param>
        /// <param name="contentType">Content type of the service</param>
        public static void AddService<T>(T serviceInstance, IPropertyOwner propertyOwner, IContentType contentType) where T : class
        {
            var sm = ServiceManager.FromPropertyOwner(propertyOwner);
            Debug.Assert(sm != null);

            sm.AddService<T>(serviceInstance, contentType);
        }

        public static void RemoveService<T>(IPropertyOwner propertyOwner) where T : class
        {
            var sm = ServiceManager.FromPropertyOwner(propertyOwner);
            Debug.Assert(sm != null);

            sm.RemoveService<T>();
        }

        public static void RemoveService<T>(IPropertyOwner propertyOwner, IContentType contentType) where T : class
        {
            var sm = ServiceManager.FromPropertyOwner(propertyOwner);
            Debug.Assert(sm != null);

            sm.RemoveService<T>(contentType);
        }

        private T GetService<T>() where T : class
        {
            return GetService<T>(true);
        }

        private T GetService<T>(bool checkDerivation) where T : class
        {
            lock (_lock)
            {
                object service = null;

                if (!_servicesByType.TryGetValue(typeof(T), out service) && checkDerivation)
                {
                    // try walk through and cast. Perhaps someone is asking for IFoo
                    // that is implemented on class Bar but Bar was added as Bar, not as IFoo
                    foreach (var kvp in _servicesByType)
                    {
                        service = kvp.Value as T;
                        if (service != null)
                            break;
                    }
                }

                return service as T;
            }
        }

        private T GetService<T>(IContentType contentType) where T : class
        {
            lock (_lock)
            {
                object service = null;

                _servicesByContentType.TryGetValue(Tuple.Create(typeof(T), contentType.TypeName), out service);
                if (service != null)
                    return service as T;

                // Try walking through and cast. Perhaps someone is asking for IFoo
                // that is implemented on class Bar but Bar was added as Bar, not as IFoo
                foreach (var kvp in _servicesByContentType)
                {
                    if (String.Compare(kvp.Key.Item2, contentType.TypeName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        service = kvp.Value as T;
                        if (service != null)
                            return service as T;
                    }
                }

                // iterate through base types since Razor, PHP and ASP.NET content type derive from HTML
                foreach (var ct in contentType.BaseTypes)
                {
                    service = GetService<T>(ct);
                    if (service != null)
                        break;
                }

                return service as T;
            }
        }

        private ICollection<T> GetAllServices<T>() where T : class
        {
            var list = new List<T>();

            lock (_lock)
            {
                foreach (var kvp in _servicesByType)
                {
                    var service = kvp.Value as T;
                    if (service != null)
                        list.Add(service);
                }
            }

            return list;
        }

        private void AddService<T>(T serviceInstance) where T : class
        {
            bool added = false;

            lock (_lock)
            {
                if (GetService<T>(false) == null)
                {
                    _servicesByType.Add(typeof(T), serviceInstance);
                    added = true;
                }
            }

            Debug.Assert(added);
            if (added)
            {
                FireServiceAdded(typeof(T), serviceInstance);
            }
        }

        private void AddService<T>(T serviceInstance, IContentType contentType) where T : class
        {
            bool added = false;

            lock (_lock)
            {
                if (GetService<T>(contentType) == null)
                {
                    _servicesByContentType.Add(Tuple.Create(typeof(T), contentType.TypeName), serviceInstance);
                    added = true;
                }
            }

            Debug.Assert(added);
            if (added)
            {
                FireServiceAdded(typeof(T), serviceInstance);
            }
        }

        private void RemoveService<T>() where T : class
        {
            bool foundServiceInstance = false;
            object serviceInstance;

            lock (_lock)
            {
                foundServiceInstance = _servicesByType.TryGetValue(typeof(T), out serviceInstance);

                if (foundServiceInstance)
                {
                    _servicesByType.Remove(typeof(T));
                }
            }

            if (foundServiceInstance)
            {
                FireServiceRemoved(typeof(T), serviceInstance);
            }
            else
            {
                Debug.Assert(false, "Unable to find service " + typeof(T).Name + " to remove from the ServiceManager!");
            }
        }

        private void RemoveService<T>(IContentType contentType) where T : class
        {
            bool foundServiceInstance = false;
            object serviceInstance;

            lock (_lock)
            {
                Tuple<Type, string> desiredKey = Tuple.Create(typeof(T), contentType.TypeName);

                foundServiceInstance = _servicesByContentType.TryGetValue(desiredKey, out serviceInstance);

                if (foundServiceInstance)
                {
                    _servicesByContentType.Remove(desiredKey);
                }
            }

            if (foundServiceInstance)
            {
                FireServiceRemoved(typeof(T), serviceInstance);
            }
            else
            {
                Debug.Assert(false, "Unable to find service " + typeof(T).Name + " to remove from the ServiceManager!");
            }
        }

        private void FireServiceAdded(Type serviceType, object serviceInstance)
        {
            if (ServiceAdded != null)
            {
                Debug.Assert(Thread.CurrentThread == EditorShell.Shell.MainThread);
                ServiceAdded(this, new ServiceManagerEventArgs(serviceType, serviceInstance));
            }
        }

        private void FireServiceRemoved(Type serviceType, object serviceInstance)
        {
            if (ServiceRemoved != null)
            {
                Debug.Assert(Thread.CurrentThread == EditorShell.Shell.MainThread);
                ServiceRemoved(this, new ServiceManagerEventArgs(serviceType, serviceInstance));
            }
        }

        public void Dispose()
        {
            Debug.Assert(_propertyOwner != null);
            if (_propertyOwner != null)
            {
                _propertyOwner.Properties.RemoveProperty(typeof(ServiceManager));

                _servicesByType.Clear();
                _servicesByContentType.Clear();

                _propertyOwner = null;
            }
        }
    }
}