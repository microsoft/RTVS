// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Containers {
    public interface IContainerService {
        ContainerServiceStatus GetServiceStatus();
        Task<IContainer> GetContainerAsync(string containerId, CancellationToken ct);
        Task<IEnumerable<IContainer>> ListContainersAsync(bool allContainers, CancellationToken ct);
        Task<IEnumerable<ContainerImage>> ListImagesAsync(bool allImages, CancellationToken ct);
        Task<IContainer> CreateContainerFromFileAsync(BuildImageParameters buildOptions, CancellationToken ct);
        Task<IContainer> CreateContainerAsync(ContainerCreateParameters createParams, CancellationToken ct);
        Task DeleteContainerAsync(string containerId, CancellationToken ct);
        Task StartContainerAsync(string containerId, CancellationToken ct);
        Task StopContainerAsync(string containerId, CancellationToken ct);
    }
}
