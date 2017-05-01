// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.R.Components.Plots.ViewModel {
    public interface IRPlotHistoryEntryViewModel {
        IRPlot Plot { get; }
        object PlotImage { get; set; }
        Task ActivatePlotAsync();
        void RefreshDeviceName();
    }
}
