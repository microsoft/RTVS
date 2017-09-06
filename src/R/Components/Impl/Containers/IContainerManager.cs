// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Containers;

namespace Microsoft.R.Components.Containers {
    public interface IContainerManager : IDisposable {
        IReadOnlyList<IContainer> GetContainers();
        IReadOnlyList<IContainer> GetRunningContainers();
        IDisposable SubscribeOnChanges(Action containersChanged);

        Task StartAsync(string containerId, CancellationToken cancellationToken);
        Task StopAsync(string containerId, CancellationToken cancellationToken);
        Task DeleteAsync(string containerId, CancellationToken cancellationToken);
    }
}