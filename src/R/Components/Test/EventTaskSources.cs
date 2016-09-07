// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Tasks;
using Microsoft.R.Components.Plots;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Test {
    internal static class EventTaskSources {
        public static class IRSession {
            public static readonly EventTaskSource<Host.Client.IRSession, EventArgs> Mutated =
                new EventTaskSource<Host.Client.IRSession, EventArgs>(
                    (o, e) => o.Mutated += e,
                    (o, e) => o.Mutated -= e);

            public static readonly EventTaskSource<Host.Client.IRSession, RAfterRequestEventArgs> AfterRequest =
                new EventTaskSource<Host.Client.IRSession, RAfterRequestEventArgs>(
                    (o, e) => o.AfterRequest += e,
                    (o, e) => o.AfterRequest -= e);
        }

        public static class IRPlotManager {
            public static readonly EventTaskSource<Components.Plots.IRPlotManager, RPlotDeviceEventArgs> ActiveDeviceChanged =
                new EventTaskSource<Components.Plots.IRPlotManager, RPlotDeviceEventArgs>(
                    (o, e) => o.ActiveDeviceChanged += e,
                    (o, e) => o.ActiveDeviceChanged -= e);

            public static readonly EventTaskSource<Components.Plots.IRPlotManager, RPlotDeviceEventArgs> DeviceAdded =
                new EventTaskSource<Components.Plots.IRPlotManager, RPlotDeviceEventArgs>(
                    (o, e) => o.DeviceAdded += e,
                    (o, e) => o.DeviceAdded -= e);

            public static readonly EventTaskSource<Components.Plots.IRPlotManager, RPlotDeviceEventArgs> DeviceRemoved =
                new EventTaskSource<Components.Plots.IRPlotManager, RPlotDeviceEventArgs>(
                    (o, e) => o.DeviceRemoved += e,
                    (o, e) => o.DeviceRemoved -= e);
        }

        public static class IRPlotDevice {
            public static readonly EventTaskSource<Components.Plots.IRPlotDevice, RPlotDeviceEventArgs> DeviceNumChanged =
                new EventTaskSource<Components.Plots.IRPlotDevice, RPlotDeviceEventArgs>(
                    (o, e) => o.DeviceNumChanged += e,
                    (o, e) => o.DeviceNumChanged -= e);

            public static readonly EventTaskSource<Components.Plots.IRPlotDevice, RPlotDeviceEventArgs> LocatorModeChanged =
                new EventTaskSource<Components.Plots.IRPlotDevice, RPlotDeviceEventArgs>(
                    (o, e) => o.LocatorModeChanged += e,
                    (o, e) => o.LocatorModeChanged -= e);

            public static readonly EventTaskSource<Components.Plots.IRPlotDevice, RPlotEventArgs> PlotAddedOrUpdated =
                new EventTaskSource<Components.Plots.IRPlotDevice, RPlotEventArgs>(
                    (o, e) => o.PlotAddedOrUpdated += e,
                    (o, e) => o.PlotAddedOrUpdated -= e);

            public static readonly EventTaskSource<Components.Plots.IRPlotDevice, RPlotEventArgs> PlotRemoved =
                new EventTaskSource<Components.Plots.IRPlotDevice, RPlotEventArgs>(
                    (o, e) => o.PlotRemoved += e,
                    (o, e) => o.PlotRemoved -= e);

            public static readonly EventTaskSource<Components.Plots.IRPlotDevice, EventArgs> Cleared =
                new EventTaskSource<Components.Plots.IRPlotDevice, EventArgs>(
                    (o, e) => o.Cleared += e,
                    (o, e) => o.Cleared -= e);
        }
    }
}
