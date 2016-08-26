// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Plots.Implementation.View;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Plots.Implementation {
    public class RPlotDeviceVisualComponent : IRPlotDeviceVisualComponent {
        private readonly DisposableBag _disposableBag;
        private ICoreShell Shell { get; }
        private IRPlotManager PlotManager { get; }

        public RPlotDeviceVisualComponent(IRPlotManager plotManager, AsyncCommandController controller, IRPlotDeviceViewModel viewModel, IVisualComponentContainer<IRPlotDeviceVisualComponent> container, IRSettings settings, ICoreShell coreShell) {
            PlotManager = plotManager;
            Controller = controller;
            ViewModel = viewModel;
            Container = container;
            Shell = coreShell;

            var control = new RPlotDeviceControl {
                DataContext = ViewModel,
            };

            Control = control;

            _disposableBag = DisposableBag.Create<RPlotDeviceVisualComponent>()
                .Add(() => control.ContextMenuRequested -= Control_ContextMenuRequested)
                .Add(() => ViewModel.DeviceNameChanged -= ViewModel_DeviceNameChanged)
                .Add(() => ViewModel.LocatorModeChanged -= ViewModel_LocatorModeChanged)
                .Add(() => ViewModel.PlotChanged += ViewModel_PlotChanged)
                .Add(() => PlotManager.ActiveDeviceChanged += PlotManager_ActiveDeviceChanged);

            control.ContextMenuRequested += Control_ContextMenuRequested;
            ViewModel.DeviceNameChanged += ViewModel_DeviceNameChanged;
            ViewModel.LocatorModeChanged += ViewModel_LocatorModeChanged;
            ViewModel.PlotChanged += ViewModel_PlotChanged;
            PlotManager.ActiveDeviceChanged += PlotManager_ActiveDeviceChanged;
        }

        public IRPlotDeviceViewModel ViewModel { get; }

        /// <summary>
        /// Device properties to use when running tests without UI.
        /// </summary>
        public Nullable<PlotDeviceProperties> TestDeviceProperties { get; set; }

        public ICommandTarget Controller { get; }

        public FrameworkElement Control { get; }

        public IVisualComponentContainer<IVisualComponent> Container { get; }

        public void Dispose() {
            _disposableBag.TryMarkDisposed();
        }

        public PlotDeviceProperties GetDeviceProperties() {
            if (TestDeviceProperties.HasValue) {
                return TestDeviceProperties.Value;
            } else {
                return ((RPlotDeviceControl)Control).GetPlotWindowProperties();
            }
        }

        public async Task UnassignAsync() {
            await ViewModel.UnassignAsync();
            Container.UpdateCommandStatus(false);
        }

        private void Control_ContextMenuRequested(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Container.ShowContextMenu(RPlotCommandIds.PlotDeviceContextMenu, GetPosition(e, (FrameworkElement)sender));
        }

        private void ViewModel_PlotChanged(object sender, EventArgs e) {
            Container.UpdateCommandStatus(false);
        }

        private void ViewModel_LocatorModeChanged(object sender, EventArgs e) {
            Shell.DispatchOnUIThread(() => {
                UpdateCaption();
                UpdateStatus();
            });
        }

        private void ViewModel_DeviceNameChanged(object sender, EventArgs e) {
            Shell.DispatchOnUIThread(() => {
                UpdateCaption();
            });
        }

        private void PlotManager_ActiveDeviceChanged(object sender, EventArgs e) {
            Shell.DispatchOnUIThread(() => {
                UpdateCaption();
            });
        }

        private void UpdateCaption() {
            if (!string.IsNullOrEmpty(ViewModel.DeviceName)) {
                string format = ViewModel.LocatorMode ? Resources.Plots_WindowCaptionLocatorActive : ViewModel.IsDeviceActive ? format = Resources.Plots_WindowCaptionDeviceActive : Resources.Plots_WindowCaptionDevice;
                Container.CaptionText = string.Format(CultureInfo.CurrentUICulture, format, ViewModel.DeviceName);
            } else {
                Container.CaptionText = Resources.Plots_WindowCaptionNoDevice;
            }
        }

        private void UpdateStatus() {
            Container.StatusText = ViewModel.LocatorMode ? Resources.Plots_StatusLocatorActive : string.Empty;
        }

        private static Point GetPosition(InputEventArgs e, FrameworkElement fe) {
            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs != null) {
                return mouseEventArgs.GetPosition(fe);
            }

            var touchEventArgs = e as TouchEventArgs;
            if (touchEventArgs != null) {
                return touchEventArgs.GetTouchPoint(fe).Position;
            }

            return new Point(0, 0);
        }
    }
}
