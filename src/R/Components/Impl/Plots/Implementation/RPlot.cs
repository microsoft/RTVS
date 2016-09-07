// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows.Media.Imaging;

namespace Microsoft.R.Components.Plots.Implementation {
    internal class RPlot : IRPlot {
        public RPlot(IRPlotDevice parentDevice, Guid plotId, BitmapImage image) {
            ParentDevice = parentDevice;
            PlotId = plotId;
            Image = image;
        }

        public IRPlotDevice ParentDevice { get; }

        public Guid PlotId { get; }

        public BitmapImage Image { get; set; }
    }
}
