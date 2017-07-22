// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Containers {
    public interface IContainerService {
        ContainerServiceStatus GetServiceStatus();
        Task<IContainer> GetContainerAsync(string containerId, CancellationToken ct);
        Task<IEnumerable<string>> ListContainersAsync(bool allContainers, CancellationToken ct);
        Task<IContainer> CreateContainerAsync(ContainerCreateParameters createParams, CancellationToken ct);
        Task DeleteContainerAsync(IContainer container, CancellationToken ct);
        Task StartContainerAsync(IContainer container, CancellationToken ct);
        Task StopContainerAsync(IContainer container, CancellationToken ct);
    }
}
