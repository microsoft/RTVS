// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Components.Plots.Implementation.View;
using Microsoft.R.Components.Plots.Implementation.ViewModel;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Plots.Implementation {
    public class RPlotDeviceVisualComponent : IRPlotDeviceVisualComponent {
        private readonly DisposableBag _disposableBag;
        private readonly IRPlotDeviceViewModel _viewModel;
        private readonly IMainThread _mainThread;

        public RPlotDeviceVisualComponent(IRPlotManager plotManager
            , int instanceId
            , IVisualComponentContainer<IRPlotDeviceVisualComponent> container
            , IServiceContainer services) {
            Check.ArgumentNull(nameof(plotManager), plotManager);
            Check.ArgumentNull(nameof(plotManager), plotManager);

            _mainThread = services.MainThread();
            _viewModel = new RPlotDeviceViewModel(plotManager, _mainThread, instanceId);

            var control = new RPlotDeviceControl {
                DataContext = _viewModel,
            };

            _disposableBag = DisposableBag.Create<RPlotDeviceVisualComponent>()
                .Add(() => control.ContextMenuRequested -= Control_ContextMenuRequested)
                .Add(() => _viewModel.DeviceNameChanged -= ViewModel_DeviceNameChanged)
                .Add(() => _viewModel.LocatorModeChanged -= ViewModel_LocatorModeChanged)
                .Add(() => _viewModel.PlotChanged -= ViewModel_PlotChanged)
                .Add(() => plotManager.ActiveDeviceChanged -= PlotManager_ActiveDeviceChanged);

            control.ContextMenuRequested += Control_ContextMenuRequested;
            _viewModel.DeviceNameChanged += ViewModel_DeviceNameChanged;
            _viewModel.LocatorModeChanged += ViewModel_LocatorModeChanged;
            _viewModel.PlotChanged += ViewModel_PlotChanged;
            plotManager.ActiveDeviceChanged += PlotManager_ActiveDeviceChanged;

            Control = control;
            Container = container;
        }


        /// <summary>
        /// Device properties to use when running tests without UI.
        /// </summary>
        public PlotDeviceProperties? TestDeviceProperties { get; set; }
        public FrameworkElement Control { get; }
        public IVisualComponentContainer<IVisualComponent> Container { get; }

        public bool HasPlot => _viewModel.PlotImage != null;
        public int ActivePlotIndex => _viewModel.Device == null ? -1 : _viewModel.Device.ActiveIndex;
        public int PlotCount => _viewModel.Device == null ? 0 : _viewModel.Device.PlotCount;
        public string DeviceName => _viewModel.DeviceName;
        public bool IsDeviceActive => _viewModel.IsDeviceActive;
        public int InstanceId=> _viewModel.InstanceId;
        public IRPlotDevice Device => _viewModel.Device;
        public IRPlot ActivePlot => _viewModel.Device.ActivePlot;

        public PlotDeviceProperties GetDeviceProperties() {
            if (TestDeviceProperties.HasValue) {
                return TestDeviceProperties.Value;
            } else {
                return ((RPlotDeviceControl)Control).GetPlotWindowProperties();
            }
        }

        public void Assign(IRPlotDevice device) {
            _viewModel.Assign(device);
            Container.UpdateCommandStatus(false);
        }

        public void Unassign() {
            _viewModel.Unassign();
            Container.UpdateCommandStatus(false);
        }

        public async Task ResizePlotAsync(int pixelWidth, int pixelHeight, int resolution) {
            await _viewModel.ResizePlotAsync(pixelWidth, pixelHeight, resolution);
        }

        public void Dispose() {
            _disposableBag.TryDispose();
        }

        private void Control_ContextMenuRequested(object sender, PointEventArgs e) {
            Container.ShowContextMenu(RPlotCommandIds.PlotDeviceContextMenu, e.Point);
        }

        private void ViewModel_PlotChanged(object sender, EventArgs e) {
            Container.UpdateCommandStatus(false);
        }

        private void ViewModel_LocatorModeChanged(object sender, EventArgs e) {
            _mainThread.Post(() => {
                UpdateCaption();
                UpdateStatus();
            });
        }

        private void ViewModel_DeviceNameChanged(object sender, EventArgs e) {
            _mainThread.Post(() => {
                UpdateCaption();
            });
        }

        private void PlotManager_ActiveDeviceChanged(object sender, EventArgs e) {
            _mainThread.Post(() => {
                UpdateCaption();
            });
        }

        private void UpdateCaption() {
            if (!string.IsNullOrEmpty(_viewModel.DeviceName)) {
                string format = _viewModel.LocatorMode ? Resources.Plots_WindowCaptionLocatorActive : _viewModel.IsDeviceActive ? format = Resources.Plots_WindowCaptionDeviceActive : Resources.Plots_WindowCaptionDevice;
                Container.CaptionText = string.Format(CultureInfo.CurrentCulture, format, _viewModel.DeviceName);
            } else {
                Container.CaptionText = Resources.Plots_WindowCaptionNoDevice;
            }
        }

        private void UpdateStatus() {
            Container.StatusText = _viewModel.LocatorMode ? Resources.Plots_StatusLocatorActive : string.Empty;
        }
    }
}
