// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Plots {
    public interface IRPlotDeviceVisualComponent : IVisualComponent {
        PlotDeviceProperties GetDeviceProperties();
        Task AssignAsync(IRPlotDevice device);
        Task UnassignAsync();
        int InstanceId { get; }
        bool HasPlot { get; }
        bool LocatorMode { get; }
        int ActivePlotIndex { get; }
        int PlotCount { get; }
        string DeviceName { get; }
        bool IsDeviceActive { get; }
        Task ActivateDeviceAsync();
        Task ExportToBitmapAsync(string deviceName, string outputFilePath);
        Task ExportToMetafileAsync(string outputFilePath);
        Task ExportToPdfAsync(string outputFilePath);
        Task RemoveActivePlotAsync();
        Task ClearAllPlotsAsync();
        Task NextPlotAsync();
        Task PreviousPlotAsync();
        Task<LocatorResult> StartLocatorModeAsync(CancellationToken ct);
        void EndLocatorMode();
        Task CopyPlotFromAsync(Guid sourceDeviceId, Guid sourcePlotId, bool isMove);
        void CopyToClipboard(bool cut);
        void ClickPlot(int x, int y);
    }
}
