// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows.Media.Imaging;

namespace Microsoft.R.Components.Plots {
    public interface IRPlotDevice {
        Guid DeviceId { get; }
        int DeviceNum { get; set; }
        int PlotCount { get; }
        int ActiveIndex { get; }
        IRPlot ActivePlot { get; }
        bool LocatorMode { get; set; }

        event EventHandler<RPlotDeviceEventArgs> DeviceNumChanged;
        event EventHandler<RPlotDeviceEventArgs> LocatorModeChanged;
        event EventHandler<RPlotEventArgs> PlotAddedOrUpdated;
        event EventHandler<RPlotEventArgs> PlotRemoved;

        IRPlot GetPlotAt(int index);
        IRPlot Find(Guid plotId);
        void AddOrUpdate(Guid plotId, BitmapImage image);
        void Remove(IRPlot plot);
        void Clear();
    }
}
