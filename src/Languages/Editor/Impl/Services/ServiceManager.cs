// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.EditorHelpers;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.R.Components.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Services {
    public sealed class ServiceManager : ServiceManagerBase {
        private readonly ICoreShell _shell;

        public static void AdviseServiceAdded<T>(IPropertyOwner propertyOwner, Action<T> callback) where T : class {
            var sm = FromPropertyOwner(propertyOwner);

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

        public static void AdviseServiceRemoved<T>(IPropertyOwner propertyOwner, Action<T> callback) where T : class {
            var sm = FromPropertyOwner(propertyOwner);

            EventHandler<ServiceManagerEventArgs> onServiceRemoved = null;
            onServiceRemoved = (sender, eventArgs) => {
                if (eventArgs.ServiceType == typeof (T)) {
                    callback(eventArgs.Service as T);
                    sm.ServiceRemoved -= onServiceRemoved;
                }
            };

            sm.ServiceRemoved += onServiceRemoved;
        }

        private ServiceManager(IPropertyOwner propertyOwner) : base(propertyOwner) {
            _shell = EditorShell.Current;
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

        /// <summary>
        /// Returns service manager attached to a given Property owner
        /// </summary>
        /// <param name="propertyOwner">Property owner</param>
        /// <returns>Service manager instance</returns>
        public static ServiceManager FromPropertyOwner(IPropertyOwner propertyOwner) {
            return (ServiceManager)FromPropertyOwner(propertyOwner, po => new ServiceManager(po));
        }
        
        /// <summary>
        /// Add service to a service manager associated with a particular Property owner
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="serviceInstance">Service instance</param>
        /// <param name="propertyOwner">Property owner</param>
        public static void AddService<T>(T serviceInstance, IPropertyOwner propertyOwner) where T : class {
            var sm = FromPropertyOwner(propertyOwner);
            Debug.Assert(sm != null);

            sm.AddService(serviceInstance);
        }

        public static void RemoveService<T>(IPropertyOwner propertyOwner) where T : class {
            if (propertyOwner != null) {
                var sm = FromPropertyOwner(propertyOwner);
                Debug.Assert(sm != null);

                sm.RemoveService<T>();
            }
        }
    }
}