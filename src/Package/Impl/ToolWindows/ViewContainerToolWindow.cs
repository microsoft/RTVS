// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.View;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Package.Windows;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.ToolWindows {
    internal class ViewContainerToolWindow : RToolWindowPane, IToolWindow {
        public static readonly DependencyProperty ToolWindowProperty = DependencyProperty.RegisterAttached(
            "ImportedNamespaces", typeof(RToolWindowPane), typeof(ViewContainerToolWindow));

        public static RToolWindowPane GetToolWindow(FrameworkElement view) => (RToolWindowPane)view.GetValue(ToolWindowProperty);
        public static void SetToolWindow(FrameworkElement view, RToolWindowPane value) => view.SetValue(ToolWindowProperty, value);
        
        protected IOleCommandTarget OleCommandTarget { get; set; }
        public IServiceContainer Services { get; set; }
        public object ViewModel { get; set; }

        public override void OnToolWindowCreated() {
            Dispatcher.CurrentDispatcher.VerifyAccess();
            var commandUiGuid = VSConstants.GUID_TextEditorFactory;
            ((IVsWindowFrame)Frame).SetGuidProperty((int)__VSFPROPID.VSFPROPID_InheritKeyBindings, ref commandUiGuid);
            base.OnToolWindowCreated();
        }

        protected override object GetService(Type serviceType) 
            => serviceType.IsEquivalentTo(typeof(IOleCommandTarget)) ? OleCommandTarget : base.GetService(serviceType);

        protected override void Dispose(bool disposing) {
            if (!disposing) {
                return;
            }

            (Content as IDisposable)?.Dispose();
            (ViewModel as IDisposable)?.Dispose();
            Content = null;
            OleCommandTarget = null;
            base.Dispose(true);
        }

        public void Show(bool focus, bool immediate) {
            Dispatcher.CurrentDispatcher.VerifyAccess();
            ToolWindowUtilities.ShowToolWindow(this, Services.MainThread(), focus, immediate);
        }
    }
}