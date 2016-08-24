// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Wpf;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;

namespace Microsoft.R.Components.Plots.Implementation.ViewModel {
    public class RPlotDeviceViewModel : BindableBase, IRPlotDeviceViewModel {
        private readonly IRPlotManager _plotManager;
        private readonly IRSession _session;

        private int _deviceNum;
        private readonly DelayedAsyncAction _resizeAction = new DelayedAsyncAction(250);
        private BitmapImage _plotImage;
        private bool _locatorMode;
        private bool _showWatermark;
        private bool _showError;
        private Guid _deviceId;
        private TaskCompletionSource<LocatorResult> _locatorTcs;

        private int _lastPixelWidth = -1;
        private int _lastPixelHeight = -1;
        private int _lastResolution = -1;

        public int InstanceId { get; }
        public int? SessionProcessId => _session.ProcessId;

        public Guid ActivePlotId { get; set; }

        public int ActivePlotIndex { get; private set; }

        public int PlotCount { get; private set; }

        public bool IsInLocatorMode => _locatorTcs != null;

        public event EventHandler DeviceNameChanged;
        public event EventHandler PlotChanged;
        public event EventHandler LocatorModeChanged;

        public RPlotDeviceViewModel(IRPlotManager plotManager, IRSession session, int instanceId) {
            if (plotManager == null) {
                throw new ArgumentNullException(nameof(plotManager));
            }

            if (session == null) {
                throw new ArgumentNullException(nameof(session));
            }

            _plotManager = plotManager;
            _session = session;
            _deviceNum = -1;
            InstanceId = instanceId;
            _showWatermark = true;
            ActivePlotIndex = -1;
            PlotCount = 0;
        }

        public bool HasPlot {
            get { return _plotImage != null; }
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

        public Guid DeviceId {
            get { return _deviceId; }
        }

        public string DeviceName {
            get {
                if (_deviceNum > 0) {
                    return string.Format(Resources.Plots_DeviceName, _deviceNum);
                } else {
                    return string.Empty;
                }
            }
        }

        private int DeviceNum {
            get {
                return _deviceNum;
            }
            set {
                if (value != _deviceNum) {
                    _deviceNum = value;
                    OnPropertyChanged(nameof(DeviceName));
                    DeviceNameChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool IsDeviceActive {
            get { return _deviceId != Guid.Empty && _deviceId == _plotManager.ActiveDeviceId; }
        }

        public Task AssignAsync(Guid deviceId) {
            _deviceId = deviceId;
            return Task.CompletedTask;
        }

        public Task UnassignAsync() {
            Clear();
            ClearHistory();
            _deviceId = Guid.Empty;
            DeviceNum = 0;
            ActivePlotIndex = -1;
            PlotCount = 0;
            return Task.CompletedTask;
        }

        public async Task RefreshDeviceNameAsync() {
            if (_deviceId != Guid.Empty) {
                DeviceNum = await _plotManager.InteractiveWorkflow.RSession.GetPlotDeviceNumAsync(_deviceId);
            } else {
                DeviceNum = 0;
            }
        }

        public Task PlotMessageClearAllAsync(Guid deviceId, int deviceNum) {
            _deviceId = deviceId;
            DeviceNum = deviceNum;
            ActivePlotId = Guid.Empty;
            Clear();
            ClearHistory();
            PlotChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public Task PlotMessageLoadPlotAsync(Guid deviceId, Guid plotId, BitmapImage image, int deviceNum, int activePlotIndex, int plotCount) {
            _deviceId = deviceId;
            DeviceNum = deviceNum;
            ActivePlotId = plotId;
            LoadPlot(image);
            UpdateHistory(plotId, activePlotIndex, plotCount, image);
            PlotChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public Task PlotMessageLoadErrorAsync(Guid deviceId, Guid plotId, int deviceNum, int activePlotIndex, int plotCount) {
            _deviceId = deviceId;
            DeviceNum = deviceNum;
            ActivePlotId = plotId;
            Error();
            UpdateHistory(plotId, activePlotIndex, plotCount, null);
            PlotChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public async Task ResizePlotAsync(int pixelWidth, int pixelHeight, int resolution) {
            if (_deviceId != Guid.Empty) {
                await _plotManager.ResizeAsync(_deviceId, pixelWidth, pixelHeight, resolution);
            }
        }

        public async Task ExportToBitmapAsync(string deviceName, string outputFilePath) {
            Debug.Assert(_deviceId != Guid.Empty);
            Debug.Assert(ActivePlotId != Guid.Empty);
            await _plotManager.ExportToBitmapAsync(_deviceId, ActivePlotId, deviceName, outputFilePath, _lastPixelWidth, _lastPixelHeight, _lastResolution);
        }

        public async Task ExportToMetafileAsync(string outputFilePath) {
            Debug.Assert(_deviceId != Guid.Empty);
            Debug.Assert(ActivePlotId != Guid.Empty);
            await _plotManager.ExportToMetafileAsync(_deviceId, ActivePlotId, outputFilePath, PixelsToInches(_lastPixelWidth), PixelsToInches(_lastPixelHeight), _lastResolution);
        }

        public async Task ExportToPdfAsync(string outputFilePath) {
            Debug.Assert(_deviceId != Guid.Empty);
            await _plotManager.ExportToPdfAsync(_deviceId, ActivePlotId, outputFilePath, PixelsToInches(_lastPixelWidth), PixelsToInches(_lastPixelHeight));
        }

        public async Task RemoveActivePlotAsync() {
            var plotId = ActivePlotId;
            Debug.Assert(_deviceId != Guid.Empty);
            Debug.Assert(plotId != Guid.Empty);
            await _plotManager.RemovePlotAsync(_deviceId, plotId);
        }

        public async Task ClearAllPlotsAsync() {
            Debug.Assert(_deviceId != Guid.Empty);
            await _plotManager.RemoveAllPlotsAsync(_deviceId);
        }

        public async Task NextPlotAsync() {
            Debug.Assert(_deviceId != Guid.Empty);
            await _plotManager.NextPlotAsync(_deviceId);
        }

        public async Task PreviousPlotAsync() {
            Debug.Assert(_deviceId != Guid.Empty);
            await _plotManager.PreviousPlotAsync(_deviceId);
        }

        public async Task ActivateDeviceAsync() {
            if (_deviceId == Guid.Empty) {
                await _plotManager.NewDeviceAsync(InstanceId);
            } else {
                await _plotManager.ActivateDeviceAsync(_deviceId);
            }
        }

        public void ClickPlot(int pixelX, int pixelY) {
            if (LocatorMode) {
                var result = LocatorResult.CreateClicked(pixelX, pixelY);
                EndLocatorMode(result);
            }
        }

        public async Task CopyPlotFromAsync(Guid sourceDeviceId, Guid sourcePlotId, bool isMove) {
            if (_deviceId == Guid.Empty) {
                await _plotManager.NewDeviceAsync(InstanceId);
            }

            Debug.Assert(_deviceId != Guid.Empty);
            await _plotManager.CopyPlotAsync(sourceDeviceId, sourcePlotId, _deviceId);
            if (isMove) {
                await _plotManager.RemovePlotAsync(sourceDeviceId, sourcePlotId);
                // Removing that plot may activate the device from the removed plot,
                // which would hide this device. So we re-show it.
                await _plotManager.ShowDeviceAsync(_deviceId);
            }
        }

        public async Task<LocatorResult> StartLocatorModeAsync(CancellationToken ct) {
            _locatorTcs = new TaskCompletionSource<LocatorResult>();
            ct.Register(EndLocatorMode);

            LocatorMode = true;
            LocatorModeChanged?.Invoke(this, EventArgs.Empty);

            var task = _locatorTcs.Task;
            return await task;
        }

        public void EndLocatorMode() {
            EndLocatorMode(LocatorResult.CreateNotClicked());
        }

        public void EndLocatorMode(LocatorResult result) {
            var tcs = _locatorTcs;
            _locatorTcs = null;
            tcs?.SetResult(result);

            LocatorMode = false;
            LocatorModeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void LoadPlot(BitmapImage image) {
            PlotImage = image;
            ShowWatermark = false;
            ShowError = false;

            // Remember the size of the last plot.
            // We'll use that when exporting the plot to image/pdf.
            if (image != null) {
                _lastPixelWidth = image.PixelWidth;
                _lastPixelHeight = image.PixelHeight;
                _lastResolution = (int)Math.Round(image.DpiX);
            }
        }

        private void Clear() {
            PlotImage = null;
            ShowWatermark = true;
            ShowError = false;
        }

        private void Error() {
            PlotImage = null;
            ShowWatermark = false;
            ShowError = true;
        }

        private void ClearHistory() {
            ActivePlotIndex = -1;
            PlotCount = 0;
        }

        private void UpdateHistory(Guid plotId, int activePlotIndex, int plotCount, BitmapImage plotImage) {
            ActivePlotIndex = activePlotIndex;
            PlotCount = plotCount;
        }

        private static double PixelsToInches(int pixels) {
            return pixels / 96.0;
        }
    }
}
