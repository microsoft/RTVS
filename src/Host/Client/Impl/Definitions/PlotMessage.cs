// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;

namespace Microsoft.R.Host.Client {
    public struct PlotMessage {
        public string FilePath { get; }
        public int ActivePlotIndex { get; }
        public int PlotCount { get; }

        public PlotMessage(string filePath, int activePlotIndex, int plotCount) {
            FilePath = filePath;
            ActivePlotIndex = activePlotIndex;
            PlotCount = plotCount;
        }

        public bool IsClearAll => string.IsNullOrEmpty(FilePath);

        public bool IsPlot => new FileInfo(FilePath).Length > 0;

        /// <summary>
        /// A zero-sized file means a blank image, ie. the plot could not be rendered.
        /// </summary>
        public bool IsError => new FileInfo(FilePath).Length == 0;
    }
}
