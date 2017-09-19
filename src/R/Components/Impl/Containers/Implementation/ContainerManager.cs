// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Containers;

namespace Microsoft.R.Components.Containers.Implementation {
    internal class ContainerManager : IContainerManager {
        private readonly IContainerService _containerService;
        private readonly IFileSystem _fileSystem;
        private readonly CountdownDisposable _containersChangedCountdown;
        private ImmutableArray<IContainer> _containers;
        private ImmutableArray<IContainer> _runningContainers;
        private CancellationTokenSource _updateContainersCts;
        private event Action ContainersChanged;

        public ContainerManager(IRInteractiveWorkflow interactiveWorkflow) {
            _containerService = interactiveWorkflow.Services.GetService<IContainerService>();
            _fileSystem = interactiveWorkflow.Services.FileSystem();
            _updateContainersCts = new CancellationTokenSource();
            _containers = ImmutableArray<IContainer>.Empty;
            _runningContainers = ImmutableArray<IContainer>.Empty;
            _containersChangedCountdown = new CountdownDisposable(StartUpdatingContainers, EndUpdatingContainers);
        }

        public IReadOnlyList<IContainer> GetContainers() => _containers;
        public IReadOnlyList<IContainer> GetRunningContainers() => _runningContainers;
        public IDisposable SubscribeOnChanges(Action containersChanged) {
            ContainersChanged += containersChanged;
            _containersChangedCountdown.Increment();
            return Disposable.Create(() => {
                ContainersChanged -= containersChanged;
                _containersChangedCountdown.Decrement();
            });
        }

        public async Task StartAsync(string containerId, CancellationToken cancellationToken) {
            try {
                await _containerService.StartContainerAsync(containerId, cancellationToken);
            } finally {
                await UpdateContainersOnceAsync(cancellationToken);
            }
        }

        public async Task StopAsync(string containerId, CancellationToken cancellationToken) {
            try {
                await _containerService.StopContainerAsync(containerId, cancellationToken);
            } finally {
                await UpdateContainersOnceAsync(cancellationToken);
            }
        }

        public async Task DeleteAsync(string containerId, CancellationToken cancellationToken) {
            try {
                await _containerService.DeleteContainerAsync(containerId, cancellationToken);
            } finally {
                await UpdateContainersOnceAsync(cancellationToken);
            }
        }

        public async Task<IContainer> CreateLocalDockerAsync(string name, string username, string password, string version, CancellationToken cancellationToken = default(CancellationToken)) {
            if (string.IsNullOrWhiteSpace(version)) {
                version = "3.4.1";
            }

            var dockerImageContent = $@"FROM kvnadig/rtvsd-ub1604:{version}
RUN apt upgrade -y

RUN apt-get install -y git
RUN mkdir /tmp/rtvsfiles && cd /tmp/rtvsfiles && git clone https://github.com/karthiknadig/docker-stuff.git && cd /
RUN cd /tmp/rtvsfiles 
RUN find -name *.deb | xargs dpkg -i
RUN apt-get -f install
RUN cp /tmp/rtvsfiles/docker-stuff/server.pfx /etc/rtvs
RUN rm -R /tmp/rtvsfiles

RUN useradd --create-home {username}
RUN echo ""{username}:{password}"" | chpasswd

EXPOSE 5444";                

            var guid = dockerImageContent.ToGuid().ToString();
            var folder = Path.Combine(Path.GetTempPath(), guid);
            var filePath = Path.Combine(folder, "Dockerfile");

            if (!_fileSystem.DirectoryExists(folder)) {
                _fileSystem.CreateDirectory(folder);
                _fileSystem.WriteAllText(filePath, dockerImageContent);
            }

            try {
                return await _containerService.CreateContainerFromFileAsync(new BuildImageParameters(filePath, guid, version, name), cancellationToken);
            } finally {
                await UpdateContainersOnceAsync(cancellationToken);
            }
        }

        public void Dispose() => _updateContainersCts.Cancel();

        private void StartUpdatingContainers() => UpdateContainersAsync().DoNotWait();
        private void EndUpdatingContainers() => Interlocked.Exchange(ref _updateContainersCts, new CancellationTokenSource()).Cancel();

        private async Task UpdateContainersAsync() {
            var updateContainersCancellationToken = _updateContainersCts.Token;
            while (!updateContainersCancellationToken.IsCancellationRequested) {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(updateContainersCancellationToken);
                cts.CancelAfter(5000);
                await UpdateContainersOnceAsync(cts.Token);
                await Task.Delay(10000, updateContainersCancellationToken);
            }
        }

        private async Task UpdateContainersOnceAsync(CancellationToken token) {
            try {
                UpdateContainers(await _containerService.ListContainersAsync(true, token)); 
            } catch (OperationCanceledException) {
            } catch (ContainerServiceNotInstalledException) {
                _containersChangedCountdown.Reset();
            } catch (ContainerServiceNotRunningException) {
                _containersChangedCountdown.Reset();
            }
        }

        private void UpdateContainers(IEnumerable<IContainer> newContainers) {
            var containers = newContainers.Where(c => c.HostPorts.Any()).ToImmutableArray();
            var runningContainers = containers.RemoveRange(containers.Where(c => !c.IsRunning));
            var hasChanges = containers.Length != _containers.Length || containers.Where((t, i) => !t.Id.EqualsOrdinal(_containers[i].Id) || t.IsRunning != _containers[i].IsRunning).Any();
            _containers = containers;
            _runningContainers = runningContainers;
            if (hasChanges) {
                ContainersChanged?.Invoke();
            }
        }
    }
}