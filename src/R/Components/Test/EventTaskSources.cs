// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Tasks;
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
            public static readonly EventTaskSource<Components.Plots.IRPlotManager, EventArgs> PlotMessageReceived =
                new EventTaskSource<Components.Plots.IRPlotManager, EventArgs>(
                    (o, e) => o.PlotMessageReceived += e,
                    (o, e) => o.PlotMessageReceived -= e);

            public static readonly EventTaskSource<Components.Plots.IRPlotManager, EventArgs> ActiveDeviceChanged =
                new EventTaskSource<Components.Plots.IRPlotManager, EventArgs>(
                    (o, e) => o.ActiveDeviceChanged += e,
                    (o, e) => o.ActiveDeviceChanged -= e);

            public static readonly EventTaskSource<Components.Plots.IRPlotManager, EventArgs> LocatorModeChanged =
                new EventTaskSource<Components.Plots.IRPlotManager, EventArgs>(
                    (o, e) => o.LocatorModeChanged += e,
                    (o, e) => o.LocatorModeChanged -= e);

            public static readonly EventTaskSource<Components.Plots.IRPlotManager, EventArgs> DeviceCreateMessageReceived =
                new EventTaskSource<Components.Plots.IRPlotManager, EventArgs>(
                    (o, e) => o.DeviceCreateMessageReceived += e,
                    (o, e) => o.DeviceCreateMessageReceived -= e);
        }
    }
}
