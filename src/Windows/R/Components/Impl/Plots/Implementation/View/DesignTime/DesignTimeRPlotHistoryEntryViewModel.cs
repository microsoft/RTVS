// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.R.Components.Plots.ViewModel;

namespace Microsoft.R.Components.Plots.Implementation.View.DesignTime {
#if DEBUG
    class DesignTimeRPlotHistoryEntryViewModel : IRPlotHistoryEntryViewModel {
        public Guid DeviceId { get; set; }
        public string DeviceName { get; set; }
        public IRPlot Plot => new RPlot(new RPlotDevice(DeviceId), PlotId, PlotImage);
        public Guid PlotId { get; set; }

        public object PlotImage { get; set; }
        public int? SessionProcessId { get; set; }
        public Task ActivatePlotAsync() => Task.CompletedTask;
        public void RefreshDeviceName() { }
    }
#endif
}
