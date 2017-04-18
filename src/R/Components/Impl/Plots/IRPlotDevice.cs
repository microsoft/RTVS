// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.Plots {
    public interface IRPlotDevice {
        Guid DeviceId { get; }
        int DeviceNum { get; set; }
        int PlotCount { get; }
        int ActiveIndex { get; }
        IRPlot ActivePlot { get; }
        bool LocatorMode { get; set; }
        int PixelWidth { get; set; }
        int PixelHeight { get; set; }
        int Resolution { get; set; }

        event EventHandler<RPlotDeviceEventArgs> DeviceNumChanged;
        event EventHandler<RPlotDeviceEventArgs> LocatorModeChanged;
        event EventHandler<RPlotEventArgs> PlotAddedOrUpdated;
        event EventHandler<RPlotEventArgs> PlotRemoved;

        /// <summary>
        /// All plots in the device were removed. This event is fired instead
        /// of multiple <see cref="PlotRemoved"/> events, since subscribers
        /// can process a clear more efficiently than multiple removes.
        /// </summary>
        event EventHandler<EventArgs> Cleared;

        IRPlot GetPlotAt(int index);
        IRPlot Find(Guid plotId);
        void AddOrUpdate(Guid plotId, object image);
        void Remove(IRPlot plot);
        void Clear();
    }
}
