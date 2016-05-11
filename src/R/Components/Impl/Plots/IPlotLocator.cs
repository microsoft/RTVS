// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Plots {
    public interface IPlotLocator {
        Task<LocatorResult> StartLocatorModeAsync(CancellationToken ct);

        void EndLocatorMode();

        bool IsInLocatorMode { get; }
    }
}
