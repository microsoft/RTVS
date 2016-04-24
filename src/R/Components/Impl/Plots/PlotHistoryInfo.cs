// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.Plots {
    public class PlotHistoryInfo {
        public PlotHistoryInfo() :
            this(-1, 0) {
        }

        public PlotHistoryInfo(int activePlotIndex, int plotCount) {
            ActivePlotIndex = activePlotIndex;
            PlotCount = plotCount;
        }

        public int ActivePlotIndex { get; set; }

        public int PlotCount { get; set; }
    }
}