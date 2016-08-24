// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Plots {
    public interface IRPlotDeviceVisualComponent : IVisualComponent {
        IRPlotDeviceViewModel ViewModel { get; }
        PlotDeviceProperties GetDeviceProperties();
        Task UnassignAsync();
    }
}
