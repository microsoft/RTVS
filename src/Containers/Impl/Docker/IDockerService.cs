// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Containers.Docker {
    public interface IDockerService {
        Task<string> BuildImageAsync(string buildOptions, CancellationToken ct);
        Task<IEnumerable<IContainer>> ListContainersAsync(bool getAll = true, CancellationToken ct = default(CancellationToken));
        Task<IEnumerable<ContainerImage>> ListImagesAsync(bool getAll = true, CancellationToken ct = default(CancellationToken));
        Task<IContainer> GetContainerAsync(string containerId, CancellationToken ct);
        Task<JArray> InspectAsync(IEnumerable<string> objectIds, CancellationToken ct);
        Task<string> RepositoryLoginAsync(string username, string password, string server, CancellationToken ct);
        Task<string> RepositoryLoginAsync(RepositoryCredentials auth, CancellationToken ct);
        Task<string> RepositoryLogoutAsync(RepositoryCredentials auth, CancellationToken ct);
        Task<string> RepositoryLogoutAsync(string server, CancellationToken ct);
        Task<string> PullImageAsync(string fullImageName, CancellationToken ct);
        Task<string> CreateContainerAsync(string createOptions, CancellationToken ct);
        Task<string> DeleteContainerAsync(string containerId, CancellationToken ct);
        Task<string> StartContainerAsync(string containerId, CancellationToken ct);
        Task<string> StopContainerAsync(string containerId, CancellationToken ct);
    }
}
