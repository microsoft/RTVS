// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Containers;

namespace Microsoft.R.Components.Containers {
    public interface IContainerManager : IDisposable {
        event EventHandler ContainersStatusChanged;

        ContainersStatus Status { get; }
        string Error { get; }

        IReadOnlyList<IContainer> GetContainers();
        IReadOnlyList<IContainer> GetRunningContainers();
        Task<IEnumerable<string>> GetLocalDockerVersions(CancellationToken cancellationToken = default(CancellationToken));
        IDisposable SubscribeOnChanges(Action containersChanged);

        Task StartAsync(string containerId, CancellationToken cancellationToken = default(CancellationToken));
        Task StopAsync(string containerId, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAsync(string containerId, CancellationToken cancellationToken = default(CancellationToken));
        Task<IContainer> CreateLocalDockerAsync(string name, string username, string password, string version, int port, CancellationToken cancellationToken = default(CancellationToken));
        Task<IContainer> CreateLocalDockerFromFileAsync(string name, string filePath, int port, CancellationToken cancellationToken = default(CancellationToken));
        void Restart();
    }
}