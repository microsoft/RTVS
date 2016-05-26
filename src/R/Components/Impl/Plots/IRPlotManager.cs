// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Definitions;

namespace Microsoft.R.Components.Plots {
    public interface IRPlotManager : IDisposable {
        IRPlotManagerVisualComponent VisualComponent { get; }

        IRPlotManagerVisualComponent GetOrCreateVisualComponent(IRPlotManagerVisualComponentContainerFactory visualComponentContainerFactory, int instanceId = 0);

        event EventHandler PlotChanged;

        event EventHandler LocatorModeChanged;

        int ActivePlotIndex { get; }

        int PlotCount { get; }

        IRPlotCommands Commands { get; }

        Task LoadPlotAsync(PlotMessage plot);

        Task<LocatorResult> StartLocatorModeAsync(CancellationToken ct);

        Task ClearAllPlotsAsync();

        Task RemoveCurrentPlotAsync();

        Task NextPlotAsync();

        Task PreviousPlotAsync();

        Task ResizeCurrentPlotAsync(int pixelWidth, int pixelHeight, int resolution);

        Task ExportToBitmapAsync(string deviceName, string outputFilePath);

        Task ExportToMetafileAsync(string outputFilePath);

        Task ExportToPdfAsync(string outputFilePath);

        void EndLocatorMode();

        void EndLocatorMode(LocatorResult result);

        bool IsInLocatorMode { get; }
    }
}
