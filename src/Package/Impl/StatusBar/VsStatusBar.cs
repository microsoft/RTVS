// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Wpf.Extensions;
using Microsoft.R.Components.StatusBar;
using Microsoft.VisualStudio.Shell.Interop;
using StatusBarControl = System.Windows.Controls.Primitives.StatusBar;

namespace Microsoft.VisualStudio.R.Package.StatusBar {
    internal sealed class VsStatusBar : IStatusBar {
        private readonly IServiceContainer _services;
        private readonly IIdleTimeService _idleTime;
        private readonly IVsStatusbar _vsStatusBar;
        private ItemsControl _itemsControl;
        private Visual _visualRoot;
        private bool _onIdleScheduled;

        public VsStatusBar(IServiceContainer services) {
            _services = services;
            _idleTime = services.GetService<IIdleTimeService>();
            _vsStatusBar = services.GetService<IVsStatusbar>(typeof(SVsStatusbar));
        }

        private Visual GetRootVisual() {
            var shell = _services.GetService<IVsUIShell>(typeof(SVsUIShell));
            shell.GetDialogOwnerHwnd(out IntPtr window);
            if (window == IntPtr.Zero) {
                return null;
            }

            var hwndSource = HwndSource.FromHwnd(window);
            return hwndSource?.RootVisual;
        }

        #region IStatusBar
        public IDisposable AddItem(UIElement element) {
            _services.MainThread().CheckAccess();
            EnsureItemsControlCreated();

            _itemsControl.Items.Insert(0, element);
            return Disposable.Create(() => _services.MainThread().Post(() => _itemsControl.Items.Remove(element)));
        }

        public async Task<string> GetTextAsync(CancellationToken ct = default(CancellationToken)) {
            await _services.MainThread().SwitchToAsync(ct);
            _vsStatusBar.GetText(out string text);
            return text ?? string.Empty;
        }

        public async Task SetTextAsync(string text, CancellationToken ct = default(CancellationToken)) {
            await _services.MainThread().SwitchToAsync(ct);
            _vsStatusBar.SetText(text);
        }

        public async Task<IStatusBarProgress> ShowProgressAsync(int totalSteps = 100, CancellationToken cancellationToken = default(CancellationToken)) {
            await _services.MainThread().SwitchToAsync(cancellationToken);
            var vsStatusbar = _services.GetService<IVsStatusbar>(typeof(SVsStatusbar));
            var mainThread = _services.MainThread();
            return new VsStatusBarProgress(vsStatusbar, mainThread, totalSteps);
        }
        #endregion


        private bool TryAddItemsControlToVisualRoot() {
            if (_itemsControl.Parent != null) {
                return true;
            }

            if (_visualRoot == null) {
                _visualRoot = GetRootVisual();
            }

            if (_visualRoot == null) {
                return false;
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
                return true;
            }

            // It is possible that StatusBarControl isn't created yet.
            // In this case, we will add ItemsControl directly to the dock panel that holds the StatusBarControl
            // It should be the same panel that holds VsResizeGrip
            var resizeGrip = _visualRoot.FindFirstVisualChildBreadthFirst<ResizeGrip>();

            var statusBarPanel = resizeGrip?.Parent as DockPanel;
            if (statusBarPanel == null) {
                return false;
            }

            DockPanel.SetDock(_itemsControl, Dock.Right);
            var resizeGripIndex = statusBarPanel.Children.IndexOf(resizeGrip);
            if (resizeGripIndex == statusBarPanel.Children.Count - 1) {
                statusBarPanel.Children.Add(_itemsControl);
            } else {
                statusBarPanel.Children.Insert(resizeGripIndex + 1, _itemsControl);
            }

            return true;
        }

        private void EnsureItemsControlCreated() {
            if (_itemsControl == null) {
                var frameworkElementFactory = new FrameworkElementFactory(typeof(StackPanel));
                frameworkElementFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                _itemsControl = new ItemsControl { ItemsPanel = new ItemsPanelTemplate(frameworkElementFactory) };
            }

            if (!TryAddItemsControlToVisualRoot() && !_onIdleScheduled) {
                _idleTime.Idle += OnVsIdle;
                _onIdleScheduled = true;
            }
        }

        private void OnVsIdle(object sender, EventArgs e) {
            _idleTime.Idle -= OnVsIdle;
            TryAddItemsControlToVisualRoot();
        }

        private class VsStatusBarProgress : IStatusBarProgress {
            private uint _progressCookie = 7329; // Something unique

            private readonly IVsStatusbar _vsStatusbar;
            private readonly IMainThread _mainThread;
            private readonly uint _totalSteps;
            private readonly string _originalText;

            public VsStatusBarProgress(IVsStatusbar vsStatusbar, IMainThread mainThread, int totalSteps) {
                mainThread.Assert();

                _vsStatusbar = vsStatusbar;
                _mainThread = mainThread;
                _totalSteps = (uint)totalSteps;

                _vsStatusbar.GetText(out string text);
                _vsStatusbar.Progress(ref _progressCookie, 1, string.Empty, 0, _totalSteps);
                _originalText = text;
            }

            public void Report(StatusBarProgressData value) {
                _mainThread.ExecuteOrPost(() => _vsStatusbar.Progress(ref _progressCookie, 1, value.Message, (uint)value.Step, _totalSteps));
            }

            public void Dispose() {
                _mainThread.ExecuteOrPost(DisposeOnMainThread);
            }

            private void DisposeOnMainThread() {
                _vsStatusbar.Progress(ref _progressCookie, 0, string.Empty, _totalSteps, _totalSteps);
                _vsStatusbar.SetText(_originalText);
            }
        }
    }
}
