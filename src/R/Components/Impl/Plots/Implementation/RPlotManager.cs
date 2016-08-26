// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots.Implementation.ViewModel;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;

namespace Microsoft.R.Components.Plots.Implementation {
    internal class RPlotManager : IRPlotManager {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly Action _dispose;

        private Dictionary<int, IRPlotDeviceVisualComponent> _visualComponents = new Dictionary<int, IRPlotDeviceVisualComponent>();
        private Dictionary<Guid, IRPlotDeviceVisualComponent> _assignedVisualComponents = new Dictionary<Guid, IRPlotDeviceVisualComponent>();
        private List<IRPlotDeviceVisualComponent> _unassignedVisualComponents = new List<IRPlotDeviceVisualComponent>();

        public IRPlotHistoryViewModel History { get; }

        public Guid ActiveDeviceId { get; private set; }

        public IRInteractiveWorkflow InteractiveWorkflow {
            get {
                return _interactiveWorkflow;
            }
        }

        public IRPlotHistoryVisualComponent HistoryVisualComponent { get; private set; }

        public event EventHandler<EventArgs> DeviceCreateMessageReceived;
        public event EventHandler<EventArgs> DeviceDestroyMessageReceived;
        public event EventHandler<EventArgs> PlotMessageReceived;
        public event EventHandler<EventArgs> ActiveDeviceChanged;
        public event EventHandler<EventArgs> LocatorModeChanged;


        public RPlotManager(IRSettings settings, IRInteractiveWorkflow interactiveWorkflow, Action dispose) {
            _interactiveWorkflow = interactiveWorkflow;
            _dispose = dispose;
            History = new RPlotHistoryViewModel(this);
            ActiveDeviceId = Guid.Empty;
            interactiveWorkflow.RSession.Disconnected += RSession_Disconnected;
            interactiveWorkflow.RSession.Mutated += RSession_Mutated;
        }

        public void Dispose() {
            _interactiveWorkflow.RSession.Disconnected -= RSession_Disconnected;
            _interactiveWorkflow.RSession.Mutated -= RSession_Mutated;

            var visualComponents = _visualComponents.Values.ToArray();
            foreach (var visualComponent in visualComponents) {
                visualComponent.ViewModel.LocatorModeChanged -= ViewModel_LocatorModeChanged;
                visualComponent.Dispose();
            }
            _dispose();
        }

        public IRPlotDeviceVisualComponent GetOrCreateVisualComponent(IRPlotDeviceVisualComponentContainerFactory visualComponentContainerFactory, int instanceId) {
            IRPlotDeviceVisualComponent component;
            if (_visualComponents.TryGetValue(instanceId, out component)) {
                return component;
            }

            component = visualComponentContainerFactory.GetOrCreate(this, _interactiveWorkflow.RSession, instanceId).Component;
            component.ViewModel.LocatorModeChanged += ViewModel_LocatorModeChanged;
            _visualComponents[instanceId] = component;
            return component;
        }

        private void ViewModel_LocatorModeChanged(object sender, EventArgs e) {
            LocatorModeChanged?.Invoke(this, e);
        }

        public IRPlotHistoryVisualComponent GetOrCreateVisualComponent(IRPlotHistoryVisualComponentContainerFactory visualComponentContainerFactory, int instanceId) {
            if (HistoryVisualComponent == null) {
                HistoryVisualComponent = visualComponentContainerFactory.GetOrCreate(this, instanceId).Component;
            }

            return HistoryVisualComponent;
        }

        public void RegisterVisualComponent(IRPlotDeviceVisualComponent visualComponent) {
            _unassignedVisualComponents.Add(visualComponent);
        }

        public async Task DeviceDestroyedAsync(Guid deviceId) {
            IRPlotDeviceVisualComponent component;
            if (_assignedVisualComponents.TryGetValue(deviceId, out component)) {
                // Remove the plots from history before the device view model gets unassigned
                HistoryVisualComponent?.ViewModel.RemoveAll(deviceId);

                await component.ViewModel.UnassignAsync();
                _assignedVisualComponents.Remove(deviceId);
                _unassignedVisualComponents.Add(component);
            } else {
                Debug.Assert(false, "Failed to destroy a plot visual component.");
            }

            DeviceDestroyMessageReceived?.Invoke(this, EventArgs.Empty);
        }

        public async Task LoadPlotAsync(PlotMessage plot) {
            InteractiveWorkflow.Shell.AssertIsOnMainThread();

            var visualComponent = await GetVisualComponentForDevice(plot.DeviceId);
            if (visualComponent != null) {
                visualComponent.Container.Show(focus: false, immediate: false);
                await ProcessPlotMessage(visualComponent.ViewModel, plot);
                visualComponent.Container.UpdateCommandStatus(false);
            }

            PlotMessageReceived?.Invoke(this, EventArgs.Empty);
        }

        public async Task ShowDeviceAsync(Guid deviceId) {
            var visualComponent = await GetVisualComponentForDevice(deviceId);
            if (visualComponent != null) {
                visualComponent.Container.Show(focus: false, immediate: false);
            }
        }

        public async Task<PlotDeviceProperties> DeviceCreatedAsync(Guid deviceId) {
            var visualComponent = await GetVisualComponentForDevice(deviceId);
            if (visualComponent != null) {
                visualComponent.Container.Show(focus: false, immediate: true);

                DeviceCreateMessageReceived?.Invoke(this, EventArgs.Empty);

                return visualComponent.GetDeviceProperties();
            }

            Debug.Assert(false, "Failed to create a plot visual component.");
            return PlotDeviceProperties.Default;
        }

        public async Task<LocatorResult> StartLocatorModeAsync(Guid deviceId, CancellationToken ct) {
            var visualComponent = await GetVisualComponentForDevice(deviceId);
            if (visualComponent != null) {
                visualComponent.Container.Show(focus: false, immediate: true);
            }

            return await visualComponent.ViewModel.StartLocatorModeAsync(ct);
        }

        public async Task RemoveAllPlotsAsync(Guid deviceId) {
            await TaskUtilities.SwitchToBackgroundThread();
            try {
                await _interactiveWorkflow.RSession.ClearPlotHistoryAsync(deviceId);
            } catch (RHostDisconnectedException ex) {
                throw new RPlotManagerException(Resources.Plots_TransportError, ex);
            }
        }

        public async Task RemovePlotAsync(Guid deviceId, Guid plotId) {
            await TaskUtilities.SwitchToBackgroundThread();
            try {
                await _interactiveWorkflow.RSession.RemoveCurrentPlotAsync(deviceId, plotId);
                await RemoveFromHistoryAsync(plotId);
            } catch (RHostDisconnectedException ex) {
                throw new RPlotManagerException(Resources.Plots_TransportError, ex);
            }
        }

        public async Task ActivatePlotAsync(Guid deviceId, Guid plotId) {
            if (History.AutoHide) {
                HistoryVisualComponent?.Container.Hide();
            }

            await TaskUtilities.SwitchToBackgroundThread();
            try {
                await _interactiveWorkflow.RSession.SelectPlotAsync(deviceId, plotId);
            } catch (RHostDisconnectedException ex) {
                throw new RPlotManagerException(Resources.Plots_TransportError, ex);
            }
        }

        public async Task NextPlotAsync(Guid deviceId) {
            await TaskUtilities.SwitchToBackgroundThread();
            try {
                await _interactiveWorkflow.RSession.NextPlotAsync(deviceId);
            } catch (RHostDisconnectedException ex) {
                throw new RPlotManagerException(Resources.Plots_TransportError, ex);
            }
        }

        public async Task PreviousPlotAsync(Guid deviceId) {
            await TaskUtilities.SwitchToBackgroundThread();
            try {
                await _interactiveWorkflow.RSession.PreviousPlotAsync(deviceId);
            } catch (RHostDisconnectedException ex) {
                throw new RPlotManagerException(Resources.Plots_TransportError, ex);
            }
        }

        public async Task ResizeAsync(Guid deviceId, int pixelWidth, int pixelHeight, int resolution) {
            if (!_interactiveWorkflow.RSession.IsHostRunning) {
                return;
            }

            try {
                await _interactiveWorkflow.RSession.ResizePlotAsync(deviceId, pixelWidth, pixelHeight, resolution);
            } catch (RHostDisconnectedException ex) {
                throw new RPlotManagerException(Resources.Plots_TransportError, ex);
            }
        }

        public Task ExportToBitmapAsync(Guid deviceId, Guid plotId, string deviceName, string outputFilePath, int pixelWidth, int pixelHeight, int resolution) =>
            ExportAsync(deviceId, outputFilePath, _interactiveWorkflow.RSession.ExportPlotToBitmapAsync(deviceId, plotId, deviceName, outputFilePath, pixelWidth, pixelHeight, resolution));

        public Task ExportToMetafileAsync(Guid deviceId, Guid plotId, string outputFilePath, double inchWidth, double inchHeight, int resolution) =>
            ExportAsync(deviceId, outputFilePath, _interactiveWorkflow.RSession.ExportPlotToMetafileAsync(deviceId, plotId, outputFilePath, inchWidth, inchHeight, resolution));

        public Task ExportToPdfAsync(Guid deviceId, Guid plotId, string outputFilePath, double inchWidth, double inchHeight) =>
            ExportAsync(deviceId, outputFilePath, _interactiveWorkflow.RSession.ExportToPdfAsync(deviceId, plotId, outputFilePath, inchWidth, inchHeight));

        public async Task ActivateDeviceAsync(Guid deviceId) {
            Debug.Assert(deviceId != Guid.Empty);
            await _interactiveWorkflow.RSession.ActivatePlotDeviceAsync(deviceId);
        }

        /// <summary>
        /// Get the view model for the device. For tests only.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public IRPlotDeviceViewModel GetDeviceViewModel(Guid deviceId) {
            if (deviceId == Guid.Empty) {
                throw new ArgumentOutOfRangeException(nameof(deviceId));
            }

            return _assignedVisualComponents[deviceId].ViewModel;
        }

        public async Task NewDeviceAsync(int existingInstanceId) {
            if (existingInstanceId >= 0) {
                // User wants to create a graphics device for an existing unassigned visual component.
                // Before asking the host to create a graphics device, we adjust the unassigned
                // visual component list so the desired visual component is used when the next device
                // is created.
                SetNextVisualComponent(existingInstanceId);
            }

            // Force creation of the graphics device
            try {
                await _interactiveWorkflow.RSession.NewPlotDeviceAsync();
            } catch (RHostDisconnectedException ex) {
                throw new RPlotManagerException(Resources.Plots_TransportError, ex);
            } catch (RException ex) {
                throw new RPlotManagerException(string.Format(CultureInfo.InvariantCulture, Resources.Plots_EvalError, ex.Message), ex);
            }
        }

        public async Task CopyPlotAsync(Guid sourceDeviceId, Guid sourcePlotId, Guid targetDeviceId) {
            try {
                await InteractiveWorkflow.RSession.CopyPlotAsync(sourceDeviceId, sourcePlotId, targetDeviceId);
            } catch (RHostDisconnectedException ex) {
                throw new RPlotManagerException(Resources.Plots_TransportError, ex);
            } catch (RException ex) {
                throw new RPlotManagerException(string.Format(CultureInfo.InvariantCulture, Resources.Plots_EvalError, ex.Message), ex);
            }
        }

        private async Task ExportAsync(Guid deviceId, string outputFilePath, Task<byte[]> exportTask) {
            try {
                var result = await exportTask;
                File.WriteAllBytes(outputFilePath, result);
            } catch (IOException ex) {
                throw new RPlotManagerException(ex.Message, ex);
            } catch (RHostDisconnectedException ex) {
                throw new RPlotManagerException(Resources.Plots_TransportError, ex);
            } catch (RException ex) {
                throw new RPlotManagerException(string.Format(CultureInfo.InvariantCulture, Resources.Plots_EvalError, ex.Message), ex);
            }
        }

        private async Task<IRPlotDeviceVisualComponent> GetVisualComponentForDevice(Guid deviceId) {
            IRPlotDeviceVisualComponent component;

            // If we've already assigned a plot window, reuse it
            if (_assignedVisualComponents.TryGetValue(deviceId, out component)) {
                return component;
            }

            // If we have an unused plot window, reuse it
            if (_unassignedVisualComponents.Count > 0) {
                component = _unassignedVisualComponents[0];
            }

            // If we have no plot window to reuse, create one
            if (component == null) {
                var containerFactory = InteractiveWorkflow.Shell.ExportProvider.GetExportedValue<IRPlotDeviceVisualComponentContainerFactory>();
                component = GetOrCreateVisualComponent(containerFactory, GetUnusedInstanceId(deviceId));
            }

            await component.ViewModel.AssignAsync(deviceId);

            _assignedVisualComponents[deviceId] = component;

            if (_unassignedVisualComponents.Contains(component)) {
                _unassignedVisualComponents.Remove(component);
            }

            return component;
        }

        private int GetUnusedInstanceId(Guid deviceId) {
            // Pick the lowest number that isn't known to be used.
            // Note that some plot windows may technically exist but
            // have not been created yet in this instance of VS (it creates
            // them on demand when they are made visible).
            // By always picking the lowest 'unused' number, we
            // increase our chances of reusing a tool window that hasn't
            // been created yet, if there are any, rather than unnecessarily
            // create a new plot window.
            for (int i = 0; i < int.MaxValue; i++) {
                if (!_visualComponents.ContainsKey(i)) {
                    return i;
                }
            }

            return -1;
        }

        private void SetNextVisualComponent(int existingInstanceId) {
            int[] indices = _unassignedVisualComponents.IndexWhere(component => component.ViewModel.InstanceId == existingInstanceId).ToArray();
            if (indices.Length == 1) {
                if (indices[0] > 0) {
                    // Desired visual component isn't the first in the list, so move it there.
                    var component = _unassignedVisualComponents[indices[0]];
                    _unassignedVisualComponents.RemoveAt(indices[0]);
                    _unassignedVisualComponents.Insert(0, component);
                }
            } else {
                // We didn't find the requested visual component.
                // Our internal state of unassigned visual components must be out of sync.
                Debug.Assert(false, "Could not find instance of plot window.");
            }
        }

        private async Task ProcessPlotMessage(IRPlotDeviceViewModel viewModel, PlotMessage plot) {
            InteractiveWorkflow.Shell.AssertIsOnMainThread();

            if (plot.IsClearAll) {
                await viewModel.PlotMessageClearAllAsync(plot.DeviceId, plot.DeviceNum);
                History.RemoveAll(plot.DeviceId);
            } else if (plot.IsPlot) {
                try {
                    var img = plot.ToBitmapImage();
                    await viewModel.PlotMessageLoadPlotAsync(plot.DeviceId, plot.PlotId, img, plot.DeviceNum, plot.ActivePlotIndex, plot.PlotCount);
                    History.AddOrUpdate(viewModel.DeviceName, viewModel.DeviceId, plot.PlotId, viewModel.SessionProcessId, img);
                } catch (Exception e) when (!e.IsCriticalException()) {
                    await viewModel.PlotMessageLoadErrorAsync(plot.DeviceId, plot.PlotId, plot.DeviceNum, plot.ActivePlotIndex, plot.PlotCount);
                }
            } else if (plot.IsError) {
                await viewModel.PlotMessageLoadErrorAsync(plot.DeviceId, plot.PlotId, plot.DeviceNum, plot.ActivePlotIndex, plot.PlotCount);
            }
        }

        private async Task RemoveFromHistoryAsync(Guid plotId) {
            await InteractiveWorkflow.Shell.SwitchToMainThreadAsync();
            History.Remove(plotId);
        }

        private async Task ClearHistoryAsync() {
            await InteractiveWorkflow.Shell.SwitchToMainThreadAsync();
            History.Clear();
        }

        private void RSession_Mutated(object sender, EventArgs e) {
            RSessionMutatedAsync().DoNotWait();
        }

        private async Task RSessionMutatedAsync() {
            try {
                var deviceId = await InteractiveWorkflow.RSession.GetActivePlotDeviceAsync();
                var deviceChanged = ActiveDeviceId != deviceId;
                ActiveDeviceId = deviceId;

                // Update all the devices in parallel
                var visualComponents = _visualComponents.Values.ToArray();
                var tasks = visualComponents.Select(v => v?.ViewModel.RefreshDeviceNameAsync());
                await Task.WhenAll(tasks);

                _interactiveWorkflow.ActiveWindow?.Container.UpdateCommandStatus(false);

                if (deviceChanged) {
                    ActiveDeviceChanged?.Invoke(this, EventArgs.Empty);
                }

            } catch (RException) {
            }
        }

        private void RSession_Disconnected(object sender, EventArgs e) {
            RSessionDisconnectedAsync().DoNotWait();
        }

        private async Task RSessionDisconnectedAsync() {
            foreach (var visualComponent in _assignedVisualComponents.Values) {
                await visualComponent.UnassignAsync();
                _unassignedVisualComponents.Add(visualComponent);
            }
            _assignedVisualComponents.Clear();

            await ClearHistoryAsync();
        }
    }
}
