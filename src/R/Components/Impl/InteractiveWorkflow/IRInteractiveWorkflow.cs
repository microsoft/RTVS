// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.Containers;
using Microsoft.R.Components.History;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.Plots;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.InteractiveWorkflow {
    public interface IRInteractiveWorkflow : IDisposable {
        IServiceContainer Services { get; }
        IConnectionManager Connections { get; }
        IContainerManager Containers { get; }
        IConsole Console { get; }
        IRHistory History { get; }
        IRPackageManager Packages { get; }
        IRPlotManager Plots { get; }
        IRSessionProvider RSessions { get; }
        IRSession RSession { get; }
        IRInteractiveWorkflowOperations Operations { get; }
    }
}