// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.Plots.Implementation {
    public sealed class RPlot : IRPlot {
        public RPlot(IRPlotDevice parentDevice, Guid plotId, object image) {
            ParentDevice = parentDevice;
            PlotId = plotId;
            Image = image;
        }

        public IRPlotDevice ParentDevice { get; }

        public Guid PlotId { get; }

        public object Image { get; set; }
    }
}
