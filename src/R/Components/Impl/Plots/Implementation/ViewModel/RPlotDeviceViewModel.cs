// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Wpf;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Plots.Implementation.ViewModel {
    public class RPlotDeviceViewModel : BindableBase, IRPlotDeviceViewModel {
        private readonly IRPlotManager _plotManager;
        private readonly ICoreShell _shell;
        private readonly DelayedAsyncAction _resizeAction = new DelayedAsyncAction(250);

        private IRPlotDevice _device;
        private BitmapImage _plotImage;
        private bool _locatorMode;
        private bool _showWatermark;
        private bool _showError;
        private TaskCompletionSource<LocatorResult> _locatorTcs;

        private int _lastPixelWidth = -1;
        private int _lastPixelHeight = -1;
        private int _lastResolution = -1;

        public int InstanceId { get; }

        public bool IsInLocatorMode => _locatorTcs != null;
        public IRPlotDevice Device => _device;

        public event EventHandler DeviceNameChanged;
        public event EventHandler PlotChanged;
        public event EventHandler LocatorModeChanged;

        public RPlotDeviceViewModel(IRPlotManager plotManager, ICoreShell shell, int instanceId) {
            if (plotManager == null) {
                throw new ArgumentNullException(nameof(plotManager));
            }

            if (shell == null) {
                throw new ArgumentNullException(nameof(shell));
            }

            _plotManager = plotManager;
            _shell = shell;
            InstanceId = instanceId;
            _showWatermark = true;
        }

        public BitmapImage PlotImage {
            get { return _plotImage; }
            private set { SetProperty(ref _plotImage, value); }
        }

        public bool LocatorMode {
            get { return _locatorMode; }
            private set { SetProperty(ref _locatorMode, value); }
        }

        public bool ShowWatermark {
            get { return _showWatermark; }
            private set { SetProperty(ref _showWatermark, value); }
        }

        public bool ShowError {
            get { return _showError; }
            private set { SetProperty(ref _showError, value); }
        }

        public string DeviceName {
            get {
                if (_device != null) {
                    return string.Format(CultureInfo.CurrentUICulture, Resources.Plots_DeviceName, _device.DeviceNum);
                } else {
                    return string.Empty;
                }
            }
        }

        public bool IsDeviceActive {
            get { return _device != null && _device == _plotManager.ActiveDevice; }
        }

        public Task AssignAsync(IRPlotDevice device) {
            _shell.AssertIsOnMainThread();

            _device = device;
            _device.PlotAddedOrUpdated += PlotAddedOrUpdated;
            _device.DeviceNumChanged += DeviceNumChanged;

            Refresh(_device.ActivePlot);

            return Task.CompletedTask;
        }

        public Task UnassignAsync() {
            _shell.AssertIsOnMainThread();

            if (_device != null) {
                _device.PlotAddedOrUpdated -= PlotAddedOrUpdated;
                _device.DeviceNumChanged -= DeviceNumChanged;
            }

            _device = null;
            Refresh(null);

            return Task.CompletedTask;
        }

        public async Task ResizePlotAsync(int pixelWidth, int pixelHeight, int resolution) {
            // This is safe to call from background thread
            if (_device != null) {
                await _plotManager.ResizeAsync(_device, pixelWidth, pixelHeight, resolution);
            }
        }

        public async Task ExportToBitmapAsync(string deviceName, string outputFilePath) {
            _shell.AssertIsOnMainThread();

            Debug.Assert(_device != null);
            Debug.Assert(_device.ActivePlot != null);
            await _plotManager.ExportToBitmapAsync(_device.ActivePlot, deviceName, outputFilePath, _lastPixelWidth, _lastPixelHeight, _lastResolution);
        }

        public async Task ExportToMetafileAsync(string outputFilePath) {
            _shell.AssertIsOnMainThread();

            Debug.Assert(_device != null);
            Debug.Assert(_device.ActivePlot != null);
            await _plotManager.ExportToMetafileAsync(_device.ActivePlot, outputFilePath, PixelsToInches(_lastPixelWidth), PixelsToInches(_lastPixelHeight), _lastResolution);
        }

        public async Task ExportToPdfAsync(string outputFilePath) {
            _shell.AssertIsOnMainThread();

            Debug.Assert(_device != null);
            Debug.Assert(_device.ActivePlot != null);
            await _plotManager.ExportToPdfAsync(_device.ActivePlot, outputFilePath, PixelsToInches(_lastPixelWidth), PixelsToInches(_lastPixelHeight));
        }

        public async Task RemoveActivePlotAsync() {
            _shell.AssertIsOnMainThread();

            Debug.Assert(_device != null);
            Debug.Assert(_device.ActivePlot != null);
            await _plotManager.RemovePlotAsync(_device.ActivePlot);
        }

        public async Task ClearAllPlotsAsync() {
            _shell.AssertIsOnMainThread();

            Debug.Assert(_device != null);
            await _plotManager.RemoveAllPlotsAsync(_device);
        }

        public async Task NextPlotAsync() {
            _shell.AssertIsOnMainThread();

            Debug.Assert(_device != null);
            await _plotManager.NextPlotAsync(_device);
        }

        public async Task PreviousPlotAsync() {
            _shell.AssertIsOnMainThread();

            Debug.Assert(_device != null);
            await _plotManager.PreviousPlotAsync(_device);
        }

        public async Task ActivateDeviceAsync() {
            _shell.AssertIsOnMainThread();

            if (_device == null) {
                await _plotManager.NewDeviceAsync(InstanceId);
            } else {
                await _plotManager.ActivateDeviceAsync(_device);
            }
        }

        public void ClickPlot(int pixelX, int pixelY) {
            _shell.AssertIsOnMainThread();

            if (LocatorMode) {
                var result = LocatorResult.CreateClicked(pixelX, pixelY);
                EndLocatorMode(result);
            }
        }

        public async Task CopyPlotFromAsync(Guid sourceDeviceId, Guid sourcePlotId, bool isMove) {
            _shell.AssertIsOnMainThread();

            if (_device == null) {
                await _plotManager.NewDeviceAsync(InstanceId);
            }

            Debug.Assert(_device != null);
            await _plotManager.CopyOrMovePlotFromAsync(sourceDeviceId, sourcePlotId, _device, isMove);
        }

        public async Task<LocatorResult> StartLocatorModeAsync(CancellationToken ct) {
            _shell.AssertIsOnMainThread();

            _locatorTcs = new TaskCompletionSource<LocatorResult>();
            ct.Register(EndLocatorMode);

            LocatorMode = true;
            LocatorModeChanged?.Invoke(this, EventArgs.Empty);

            _device.LocatorMode = LocatorMode;

            var task = _locatorTcs.Task;
            return await task;
        }

        public void EndLocatorMode() {
            _shell.AssertIsOnMainThread();

            EndLocatorMode(LocatorResult.CreateNotClicked());
        }

        public void EndLocatorMode(LocatorResult result) {
            _shell.AssertIsOnMainThread();

            var tcs = _locatorTcs;
            _locatorTcs = null;
            tcs?.SetResult(result);

            LocatorMode = false;
            LocatorModeChanged?.Invoke(this, EventArgs.Empty);

            _device.LocatorMode = LocatorMode;
        }

        private void DeviceNumChanged(object sender, RPlotDeviceEventArgs e) {
            DeviceNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void PlotAddedOrUpdated(object sender, RPlotEventArgs e) {
            Refresh(_device.ActivePlot);
        }

        private void Refresh(IRPlot plot) {
            _shell.DispatchOnUIThread(() => {
                if (plot != null) {
                    PlotImage = plot.Image;
                    ShowWatermark = false;
                    ShowError = plot.Image == null;

                    // Remember the size of the last plot.
                    // We'll use that when exporting the plot to image/pdf.
                    if (plot.Image != null) {
                        _lastPixelWidth = plot.Image.PixelWidth;
                        _lastPixelHeight = plot.Image.PixelHeight;
                        _lastResolution = (int)Math.Round(plot.Image.DpiX);
                    }
                } else {
                    PlotImage = null;
                    ShowWatermark = true;
                    ShowError = false;
                }

                DeviceNameChanged?.Invoke(this, EventArgs.Empty);
                PlotChanged?.Invoke(this, EventArgs.Empty);
            });
        }

        private static double PixelsToInches(int pixels) {
            return pixels / 96.0;
        }
    }
}
