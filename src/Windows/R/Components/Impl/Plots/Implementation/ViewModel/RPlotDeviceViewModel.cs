// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Common.Wpf.Controls;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Plots.Implementation.ViewModel {
    public class RPlotDeviceViewModel : BindableBase, IRPlotDeviceViewModel {
        private readonly IRPlotManager _plotManager;
        private readonly IMainThread _mainThread;

        private IRPlotDevice _device;
        private object _plotImage;
        private bool _locatorMode;
        private bool _showWatermark;
        private bool _showError;

        public int InstanceId { get; }

        public IRPlotDevice Device => _device;

        public event EventHandler DeviceNameChanged;
        public event EventHandler PlotChanged;
        public event EventHandler LocatorModeChanged;

        public RPlotDeviceViewModel(IRPlotManager plotManager, IMainThread mainThread, int instanceId) {
            Check.ArgumentNull(nameof(plotManager), plotManager);
            Check.ArgumentNull(nameof(mainThread), mainThread);

            _plotManager = plotManager;
            _mainThread = mainThread;
            InstanceId = instanceId;
            _showWatermark = true;
        }

        public object PlotImage {
            get => _plotImage;
            private set => SetProperty(ref _plotImage, value);
        }

        public bool LocatorMode {
            get => _locatorMode;
            private set => SetProperty(ref _locatorMode, value);
        }

        public bool ShowWatermark {
            get => _showWatermark;
            private set => SetProperty(ref _showWatermark, value);
        }

        public bool ShowError {
            get => _showError;
            private set => SetProperty(ref _showError, value);
        }

        public string DeviceName {
            get {
                if (_device != null) {
                    return string.Format(CultureInfo.CurrentCulture, Resources.Plots_DeviceName, _device.DeviceNum);
                } else {
                    return string.Empty;
                }
            }
        }

        public bool IsDeviceActive => _device != null && _device == _plotManager.ActiveDevice;

        public void Assign(IRPlotDevice device) {
            _mainThread.Assert();

            _device = device;
            _device.PlotAddedOrUpdated += PlotAddedOrUpdated;
            _device.Cleared += Cleared;
            _device.DeviceNumChanged += DeviceNumChanged;
            _device.LocatorModeChanged += DeviceLocatorModeChanged;

            Refresh(_device.ActivePlot);
        }

        public void Unassign() {
            _mainThread.Assert();

            if (_device != null) {
                _device.PlotAddedOrUpdated -= PlotAddedOrUpdated;
                _device.Cleared -= Cleared;
                _device.DeviceNumChanged -= DeviceNumChanged;
                _device.LocatorModeChanged -= DeviceLocatorModeChanged;
            }

            _device = null;
            Refresh(null);
        }

        public async Task ResizePlotAsync(int pixelWidth, int pixelHeight, int resolution) {
            // This is safe to call from background thread
            if (_device != null) {
                await _plotManager.ResizeAsync(_device, pixelWidth, pixelHeight, resolution);
            }
        }

        public void ClickPlot(int pixelX, int pixelY) {
            _mainThread.Assert();

            if (LocatorMode) {
                var result = LocatorResult.CreateClicked(pixelX, pixelY);
                _plotManager.EndLocatorMode(_device, result);
            }
        }

        public async Task CopyPlotFromAsync(Guid sourceDeviceId, Guid sourcePlotId, bool isMove) {
            _mainThread.Assert();

            if (_device == null) {
                await _plotManager.NewDeviceAsync(InstanceId);
            }

            Debug.Assert(_device != null);
            await _plotManager.CopyOrMovePlotFromAsync(sourceDeviceId, sourcePlotId, _device, isMove);
        }

        private void DeviceLocatorModeChanged(object sender, RPlotDeviceEventArgs e) {
            _mainThread.Post(() => {
                LocatorMode = e.Device.LocatorMode;
                LocatorModeChanged?.Invoke(this, EventArgs.Empty);
            });
        }

        private void DeviceNumChanged(object sender, RPlotDeviceEventArgs e) {
            DeviceNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void PlotAddedOrUpdated(object sender, RPlotEventArgs e) {
            Refresh(_device.ActivePlot);
        }

        private void Cleared(object sender, EventArgs e) {
            Refresh(null);
        }

        private void Refresh(IRPlot plot) {
            _mainThread.Post(() => {
                if (plot != null) {
                    PlotImage = plot.Image;
                    ShowWatermark = false;
                    ShowError = plot.Image == null;
                } else {
                    PlotImage = null;
                    ShowWatermark = true;
                    ShowError = false;
                }

                DeviceNameChanged?.Invoke(this, EventArgs.Empty);
                PlotChanged?.Invoke(this, EventArgs.Empty);
            });
        }
    }
}
