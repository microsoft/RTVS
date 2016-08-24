// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Microsoft.R.Components.Plots.ViewModel {
    public interface IRPlotHistoryEntryViewModel {
        int? SessionProcessId { get; }
        string DeviceName { get; }
        Guid DeviceId { get; }
        Guid PlotId { get; }
        BitmapImage PlotImage { get; set; }

        /// <summary>
        /// Activate the graphics device and render this plot.
        /// </summary>
        Task ActivatePlotAsync();
    }
}
