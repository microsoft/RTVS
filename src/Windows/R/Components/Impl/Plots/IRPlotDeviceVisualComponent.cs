// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Plots {
    public interface IRPlotDeviceVisualComponent : IVisualComponent {
        PlotDeviceProperties GetDeviceProperties();
        void Assign(IRPlotDevice device);
        void Unassign();
        int InstanceId { get; }
        bool HasPlot { get; }
        int ActivePlotIndex { get; }
        int PlotCount { get; }
        string DeviceName { get; }
        bool IsDeviceActive { get; }
        IRPlotDevice Device { get; }
        IRPlot ActivePlot { get; }
        // Functions below are for use by tests
        Task ResizePlotAsync(int pixelWidth, int pixelHeight, int resolution);
    }
}
