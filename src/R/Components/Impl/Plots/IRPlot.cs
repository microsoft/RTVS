// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows.Media.Imaging;

namespace Microsoft.R.Components.Plots {
    public interface IRPlot {
        IRPlotDevice ParentDevice { get; }
        Guid PlotId { get; }
        BitmapImage Image { get; set; }
    }
}
