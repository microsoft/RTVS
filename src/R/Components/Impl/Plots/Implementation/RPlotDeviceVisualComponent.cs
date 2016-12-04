// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Plots.Implementation.View;
using Microsoft.R.Components.Plots.Implementation.ViewModel;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Plots.Implementation {
    public class RPlotDeviceVisualComponent : IRPlotDeviceVisualComponent {
        private readonly DisposableBag _disposableBag;
        private readonly ICoreShell _shell;
        private readonly IRPlotManager _plotManager;
        private readonly IRPlotDeviceViewModel _viewModel;

        public RPlotDeviceVisualComponent(IRPlotManager plotManager, ICommandTarget controller, int instanceId, IVisualComponentContainer<IRPlotDeviceVisualComponent> container, ICoreShell coreShell) {
            if (plotManager == null) {
                throw new ArgumentNullException(nameof(plotManager));
            }

            if (container == null) {
                throw new ArgumentNullException(nameof(container));
            }

            if (coreShell == null) {
                throw new ArgumentNullException(nameof(coreShell));
            }

            _plotManager = plotManager;
            _viewModel = new RPlotDeviceViewModel(plotManager, coreShell, instanceId);
            _shell = coreShell;

            var control = new RPlotDeviceControl {
                DataContext = _viewModel,
            };

            _disposableBag = DisposableBag.Create<RPlotDeviceVisualComponent>()
                .Add(() => control.ContextMenuRequested -= Control_ContextMenuRequested)
                .Add(() => _viewModel.DeviceNameChanged -= ViewModel_DeviceNameChanged)
                .Add(() => _viewModel.LocatorModeChanged -= ViewModel_LocatorModeChanged)
                .Add(() => _viewModel.PlotChanged += ViewModel_PlotChanged)
                .Add(() => _plotManager.ActiveDeviceChanged += PlotManager_ActiveDeviceChanged);

            control.ContextMenuRequested += Control_ContextMenuRequested;
            _viewModel.DeviceNameChanged += ViewModel_DeviceNameChanged;
            _viewModel.LocatorModeChanged += ViewModel_LocatorModeChanged;
            _viewModel.PlotChanged += ViewModel_PlotChanged;
            _plotManager.ActiveDeviceChanged += PlotManager_ActiveDeviceChanged;

            Control = control;
            Controller = controller;
            Container = container;
        }


        /// <summary>
        /// Device properties to use when running tests without UI.
        /// </summary>
        public Nullable<PlotDeviceProperties> TestDeviceProperties { get; set; }

        public ICommandTarget Controller { get; }

        public FrameworkElement Control { get; }

        public IVisualComponentContainer<IVisualComponent> Container { get; }

        public bool HasPlot => _viewModel.PlotImage != null;

        public bool LocatorMode => _viewModel.LocatorMode;

        public int ActivePlotIndex {
            get {
                if (_viewModel.Device == null) {
                    return -1;
                }
                return _viewModel.Device.ActiveIndex;
            }
        }

        public int PlotCount {
            get {
                if (_viewModel.Device == null) {
                    return 0;
                }
                return _viewModel.Device.PlotCount;
            }
        }

        public string DeviceName {
            get {
                return _viewModel.DeviceName;
            }
        }

        public bool IsDeviceActive {
            get {
                return _viewModel.IsDeviceActive;
            }
        }

        public int InstanceId {
            get {
                return _viewModel.InstanceId;
            }
        }

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

        public Task<LocatorResult> StartLocatorModeAsync(CancellationToken ct) => _viewModel.StartLocatorModeAsync(ct);

        public void EndLocatorMode() => _viewModel.EndLocatorMode();

        public void ClickPlot(int x, int y) {
            _viewModel.ClickPlot(x, y);
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
            _shell.DispatchOnUIThread(() => {
                UpdateCaption();
                UpdateStatus();
            });
        }

        private void ViewModel_DeviceNameChanged(object sender, EventArgs e) {
            _shell.DispatchOnUIThread(() => {
                UpdateCaption();
            });
        }

        private void PlotManager_ActiveDeviceChanged(object sender, EventArgs e) {
            _shell.DispatchOnUIThread(() => {
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
