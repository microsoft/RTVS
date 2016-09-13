// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Wpf.Extensions;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.StatusBar;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using StatusBarControl = System.Windows.Controls.Primitives.StatusBar;

namespace Microsoft.VisualStudio.R.Package.StatusBar {
    [Export(typeof(IStatusBar))]
    internal class VsStatusBar : IStatusBar {
        private readonly IApplicationShell _shell;
        private ItemsControl _itemsControl;
        private Visual _visualRoot;
        private EnvDTE.DTEEvents _dteEvents;

        [ImportingConstructor]
        public VsStatusBar(IApplicationShell shell) {
            _shell = shell;
        }

        private Visual GetRootVisual() {
            var shell = _shell.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            IntPtr window;
            shell.GetDialogOwnerHwnd(out window);

            var hwndSource = HwndSource.FromHwnd(window);
            return hwndSource?.RootVisual;
        }

        public IDisposable AddItem(UIElement element) {
            _shell.AssertIsOnMainThread();
            EnsureItemsControlCreated();

            _itemsControl.Items.Insert(0, element);
            return Disposable.Create(() => _shell.DispatchOnUIThread(() => _itemsControl.Items.Remove(element)));
        }

        private void TryAddItemsControlToVisualRoot() {
            if (_visualRoot == null) {
                _visualRoot = GetRootVisual();
            }

            if (_visualRoot == null && _dteEvents == null) {
                var dte = _shell.GetGlobalService<EnvDTE.DTE>();
                if (dte == null) {
                    return;
                }

                _dteEvents = dte.Events.DTEEvents;
                _dteEvents.OnStartupComplete += OnStartupComplete;
                return;
            }

            var statusBarControl = _visualRoot.FindFirstVisualChildBreadthFirst<StatusBarControl>();
            if (statusBarControl != null) {
                var item = new StatusBarItem {
                    Content = _itemsControl,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Visibility = Visibility.Visible
                };
                DockPanel.SetDock(item, Dock.Right);
                statusBarControl.Items.Insert(0, item);
                return;
            }

            // It is possible that StatusBarControl isn't created yet.
            // In this case, we will add ItemsControl directly to the dock panel that holds the StatusBarControl
            // It should be the same panel that holds VsResizeGrip
            var resizeGrip = GetRootVisual().FindFirstVisualChildBreadthFirst<ResizeGrip>();

            var statusBarPanel = resizeGrip?.Parent as DockPanel;
            if (statusBarPanel == null) {
                return;
            }

            DockPanel.SetDock(_itemsControl, Dock.Right);
            var resizeGripIndex = statusBarPanel.Children.IndexOf(resizeGrip);
            if (resizeGripIndex == statusBarPanel.Children.Count - 1) {
                statusBarPanel.Children.Add(_itemsControl);
            } else {
                statusBarPanel.Children.Insert(resizeGripIndex + 1, _itemsControl);
            }
        }

        private void OnStartupComplete() {
            _shell.DispatchOnUIThread(() => {
                _dteEvents.OnStartupComplete -= OnStartupComplete;
                _dteEvents = null;

                EnsureItemsControlCreated();
            });
        }

        private void EnsureItemsControlCreated() {
            if (_itemsControl == null) {
                var frameworkElementFactory = new FrameworkElementFactory(typeof(StackPanel));
                frameworkElementFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                _itemsControl = new ItemsControl { ItemsPanel = new ItemsPanelTemplate(frameworkElementFactory) };
            }

            TryAddItemsControlToVisualRoot();
        }
    }
}
