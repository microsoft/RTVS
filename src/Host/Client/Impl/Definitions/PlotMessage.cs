// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Host.Client {
    public struct PlotMessage {
        public Guid DeviceId { get; }
        public Guid PlotId { get; }
        public string FilePath { get; }
        public int DeviceNum { get; }
        public int ActivePlotIndex { get; }
        public int PlotCount { get; }
        public byte[] Data { get; }

        public PlotMessage(Guid deviceId, Guid plotId, string filePath, int deviceNum, int activePlotIndex, int plotCount, byte[] data) {
            DeviceId = deviceId;
            PlotId = plotId;
            FilePath = filePath;
            DeviceNum = deviceNum;
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
