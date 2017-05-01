// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.Plots {
    public interface IRPlot {
        IRPlotDevice ParentDevice { get; }
        Guid PlotId { get; }
        object Image { get; set; }
    }
}
