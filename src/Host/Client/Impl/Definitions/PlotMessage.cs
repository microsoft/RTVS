// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;

namespace Microsoft.R.Host.Client {
    public struct PlotMessage {
        public string FilePath { get; }
        public int ActivePlotIndex { get; }
        public int PlotCount { get; }
        public byte[] Data { get; }

        public PlotMessage(string filePath, int activePlotIndex, int plotCount, byte[] data) {
            FilePath = filePath;
            ActivePlotIndex = activePlotIndex;
            PlotCount = plotCount;
            Data = data;
        }

        public bool IsClearAll => string.IsNullOrEmpty(FilePath);

        public bool IsPlot => Data.Length > 0;

        /// <summary>
        /// The plot could not be rendered id length is 0.
        /// </summary>
        public bool IsError => Data.Length == 0;
    }
}
