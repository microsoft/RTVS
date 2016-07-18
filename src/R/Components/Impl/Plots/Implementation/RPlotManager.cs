// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots.Implementation.Commands;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Extensions;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;

namespace Microsoft.R.Components.Plots.Implementation {
    internal class RPlotManager : IRPlotManager {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly Action _dispose;

        private TaskCompletionSource<LocatorResult> _locatorTcs;

        private int _lastPixelWidth = -1;
        private int _lastPixelHeight = -1;
        private int _lastResolution = -1;

        public IRPlotManagerVisualComponent VisualComponent { get; private set; }

        public IRPlotCommands Commands { get; }

        public int ActivePlotIndex { get; private set; }

        public int PlotCount { get; private set; }

        public bool IsInLocatorMode => _locatorTcs != null;

        public event EventHandler PlotChanged;

        public event EventHandler LocatorModeChanged;

        public RPlotManager(IRSessionProvider sessionProvider, IRSettings settings, IRInteractiveWorkflow interactiveWorkflow, Action dispose) {
            _interactiveWorkflow = interactiveWorkflow;
            _dispose = dispose;
            ActivePlotIndex = -1;
            PlotCount = 0;
            Commands = new PlotCommands(interactiveWorkflow);
            interactiveWorkflow.RSession.Connected += RSession_Connected;
            interactiveWorkflow.RSession.Disconnected += RSession_Disconnected;
        }

        private void RSession_Connected(object sender, EventArgs e) {
            RSessionConnectedAsync().DoNotWait();
        }

        private async Task RSessionConnectedAsync() {
            if (_lastPixelWidth > 0 && _lastPixelHeight > 0 && _lastResolution > 0) {
                try {
                    await ResizeAsync(_lastPixelWidth, _lastPixelHeight, _lastResolution);
                } catch (RException) {
                }
            }
        }

        private void RSession_Disconnected(object sender, EventArgs e) {
            ActivePlotIndex = -1;
            PlotCount = 0;
            VisualComponent?.Container.UpdateCommandStatus(false);
        }

        public IRPlotManagerVisualComponent GetOrCreateVisualComponent(IRPlotManagerVisualComponentContainerFactory visualComponentContainerFactory, int instanceId = 0) {
            if (VisualComponent != null) {
                return VisualComponent;
            }

            VisualComponent = visualComponentContainerFactory.GetOrCreate(this, _interactiveWorkflow.RSession, instanceId).Component;
            return VisualComponent;
        }

        public void Dispose() {
            _interactiveWorkflow.RSession.Connected -= RSession_Connected;
            _interactiveWorkflow.RSession.Disconnected -= RSession_Disconnected;
            VisualComponent?.Dispose();
            _dispose();
        }

        public Task LoadPlotAsync(PlotMessage plot) {
            if (VisualComponent != null) {
                if (plot.IsClearAll) {
                    VisualComponent.Clear();
                } else if (plot.IsPlot) {
                    try {
                        VisualComponent.LoadPlot(plot.ToBitmapImage());
                    } catch (Exception e) when (!e.IsCriticalException()) {
                        VisualComponent.Error();
                    }
                } else if (plot.IsError) {
                    VisualComponent.Error();
                }
            }

            ActivePlotIndex = plot.ActivePlotIndex;
            PlotCount = plot.PlotCount;
            _interactiveWorkflow.ActiveWindow?.Container.UpdateCommandStatus(false);

            PlotChanged?.Invoke(this, null);

            return Task.CompletedTask;
        }

        public async Task<LocatorResult> StartLocatorModeAsync(CancellationToken ct) {
            _locatorTcs = new TaskCompletionSource<LocatorResult>();
            ct.Register(EndLocatorMode);

            _interactiveWorkflow.Shell.DispatchOnUIThread(() => {
                SetLocatorModeUI(true);
            });

            LocatorModeChanged?.Invoke(this, null);

            var task = _locatorTcs.Task;
            return await task;
        }

        public async Task RemoveAllPlotsAsync() {
            await TaskUtilities.SwitchToBackgroundThread();
            try {
                await _interactiveWorkflow.RSession.ClearPlotHistoryAsync();
            } catch (RHostDisconnectedException ex) {
                throw new RPlotManagerException(Resources.Plots_TransportError, ex);
            }
        }

        public async Task RemoveCurrentPlotAsync() {
            await TaskUtilities.SwitchToBackgroundThread();
            try {
                await _interactiveWorkflow.RSession.RemoveCurrentPlotAsync();
            } catch (RHostDisconnectedException ex) {
                throw new RPlotManagerException(Resources.Plots_TransportError, ex);
            }
        }

        public async Task NextPlotAsync() {
            await TaskUtilities.SwitchToBackgroundThread();
            try {
                await _interactiveWorkflow.RSession.NextPlotAsync();
            } catch (RHostDisconnectedException ex) {
                throw new RPlotManagerException(Resources.Plots_TransportError, ex);
            }
        }

        public async Task PreviousPlotAsync() {
            await TaskUtilities.SwitchToBackgroundThread();
            try {
                await _interactiveWorkflow.RSession.PreviousPlotAsync();
            } catch (RHostDisconnectedException ex) {
                throw new RPlotManagerException(Resources.Plots_TransportError, ex);
            }
        }

        public async Task ResizeAsync(int pixelWidth, int pixelHeight, int resolution) {
            _lastPixelWidth = pixelWidth;
            _lastPixelHeight = pixelHeight;
            _lastResolution = resolution;

            if (!_interactiveWorkflow.RSession.IsHostRunning) {
                return;
            }

            try {
                await _interactiveWorkflow.RSession.ResizePlotAsync(pixelWidth, pixelHeight, resolution);
            } catch (RHostDisconnectedException ex) {
                throw new RPlotManagerException(Resources.Plots_TransportError, ex);
            }
        }

        public async Task ExportToBitmapAsync(string deviceName, string outputFilePath) {
            try {
                var bitmapResult = await _interactiveWorkflow.RSession.ExportToBitmapAsync(deviceName, outputFilePath, _lastPixelWidth, _lastPixelHeight, _lastResolution);
                bitmapResult.SaveRawDataToFile(outputFilePath);
            } catch (RHostDisconnectedException ex) {
                throw new RPlotManagerException(Resources.Plots_TransportError, ex);
            } catch (RException ex) {
                throw new RPlotManagerException(string.Format(CultureInfo.InvariantCulture, Resources.Plots_EvalError, ex.Message), ex);
            }
        }

        public async Task ExportToMetafileAsync(string outputFilePath) {
            try {
                var metafileResult = await _interactiveWorkflow.RSession.ExportToMetafileAsync(outputFilePath, PixelsToInches(_lastPixelWidth), PixelsToInches(_lastPixelHeight), _lastResolution);
                metafileResult.SaveRawDataToFile(outputFilePath);
            } catch (RHostDisconnectedException ex) {
                throw new RPlotManagerException(Resources.Plots_TransportError, ex);
            } catch (RException ex) {
                throw new RPlotManagerException(string.Format(CultureInfo.InvariantCulture, Resources.Plots_EvalError, ex.Message), ex);
            }
        }

        public async Task ExportToPdfAsync(string outputFilePath) {
            try {
                var pdfResult = await _interactiveWorkflow.RSession.ExportToPdfAsync(outputFilePath, PixelsToInches(_lastPixelWidth), PixelsToInches(_lastPixelHeight));
                pdfResult.SaveRawDataToFile(outputFilePath);
            } catch (RHostDisconnectedException ex) {
                throw new RPlotManagerException(Resources.Plots_TransportError, ex);
            } catch (RException ex) {
                throw new RPlotManagerException(string.Format(CultureInfo.InvariantCulture, Resources.Plots_EvalError, ex.Message), ex);
            }
        }

        public void EndLocatorMode() {
            EndLocatorMode(LocatorResult.CreateNotClicked());
        }

        public void EndLocatorMode(LocatorResult result) {
            var tcs = _locatorTcs;
            _locatorTcs = null;
            tcs?.SetResult(result);
            _interactiveWorkflow.Shell.DispatchOnUIThread(() => SetLocatorModeUI(false));
            LocatorModeChanged?.Invoke(this, null);
        }

        private void SetLocatorModeUI(bool locatorMode) {
            if (VisualComponent != null) {
                VisualComponent.SetLocatorMode(locatorMode);
            }
        }

        private static double PixelsToInches(int pixels) {
            return pixels / 96.0;
        }
    }
}
