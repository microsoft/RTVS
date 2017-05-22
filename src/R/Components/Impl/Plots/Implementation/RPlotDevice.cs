// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.R.Components.Plots.Implementation {
    public sealed class RPlotDevice : IRPlotDevice {
        private readonly List<IRPlot> _plots;
        private int _deviceNum;
        private bool _locatorMode;

        public event EventHandler<RPlotEventArgs> PlotAddedOrUpdated;
        public event EventHandler<RPlotDeviceEventArgs> DeviceNumChanged;
        public event EventHandler<RPlotEventArgs> PlotRemoved;
        public event EventHandler<EventArgs> Cleared;
        public event EventHandler<RPlotDeviceEventArgs> LocatorModeChanged;

        public RPlotDevice(Guid deviceId) {
            _plots = new List<IRPlot>();

            DeviceId = deviceId;
            ActiveIndex = -1;
        }

        public Guid DeviceId { get; }

        public int DeviceNum {
            get => _deviceNum;

            set {
                if (value != _deviceNum) {
                    _deviceNum = value;
                    DeviceNumChanged?.Invoke(this, new RPlotDeviceEventArgs(this));
                }
            }
        }

        public bool LocatorMode {
            get => _locatorMode;
            set {
                if (value != _locatorMode) {
                    _locatorMode = value;
                    LocatorModeChanged?.Invoke(this, new RPlotDeviceEventArgs(this));
                }
            }
        }

        public int PlotCount { get; private set; }
        public int ActiveIndex { get; private set; }

        public IRPlot ActivePlot => ActiveIndex >= 0 ? _plots[ActiveIndex] : null;
        public int PixelWidth { get; set; } = -1;
        public int PixelHeight { get; set; } = -1;
        public int Resolution { get; set; } = -1;

        public void AddOrUpdate(Guid plotId, object image) {
            var plot = _plots.SingleOrDefault(p => p.PlotId == plotId);
            if (plot == null) {
                plot = new RPlot(this, plotId, image);
                _plots.Add(plot);
            } else {
                plot.Image = image;
            }

            ActiveIndex = _plots.IndexOf(plot);
            PlotCount = _plots.Count;

            PlotAddedOrUpdated?.Invoke(this, new RPlotEventArgs(plot));
        }

        public void Remove(IRPlot plot) {
            _plots.Remove(plot);

            PlotCount = _plots.Count;
            if (ActiveIndex >= PlotCount) {
                ActiveIndex = PlotCount - 1;
            }

            PlotRemoved?.Invoke(this, new RPlotEventArgs(plot));
        }

        public void Clear() {
            var plots = _plots.ToArray();
            _plots.Clear();

            ActiveIndex = -1;
            PlotCount = 0;

            Cleared?.Invoke(this, new RPlotEventArgs(null));
        }

        public IRPlot GetPlotAt(int index) => _plots[index];
        public IRPlot Find(Guid plotId) => _plots.SingleOrDefault(p => p.PlotId == plotId);
    }
}
