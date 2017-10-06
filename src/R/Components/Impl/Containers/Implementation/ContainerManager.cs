// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
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
        private const string LatestVersion = "latest";
        private readonly IContainerService _containerService;
        private readonly IFileSystem _fileSystem;
        private readonly CountdownDisposable _containersChangedCountdown;
        private ImmutableArray<IContainer> _containers;
        private ImmutableArray<IContainer> _runningContainers;
        private CancellationTokenSource _updateContainersCts;
        private int _status;
        private string _error;
        private event Action ContainersChanged;

        public event EventHandler ContainersStatusChanged;
        public ContainersStatus Status {
            get => (ContainersStatus)_status;
            private set {
                if (Interlocked.Exchange(ref _status, (int) value) != (int) value) {
                    ContainersStatusChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public string Error {
            get => _error;
            private set => Interlocked.Exchange(ref _error, value);
        }

        public ContainerManager(IRInteractiveWorkflow interactiveWorkflow) {
            _containerService = interactiveWorkflow.Services.GetService<IContainerService>();
            _fileSystem = interactiveWorkflow.Services.FileSystem();
            _updateContainersCts = new CancellationTokenSource();
            _containers = ImmutableArray<IContainer>.Empty;
            _runningContainers = ImmutableArray<IContainer>.Empty;
            _containersChangedCountdown = new CountdownDisposable(StartUpdatingContainers, EndUpdatingContainers);

            Status = ContainersStatus.Running;
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

        public Task<IContainer> CreateLocalDockerAsync(string name, string username, string password, string version, int port, CancellationToken cancellationToken = default(CancellationToken)) {
            if (string.IsNullOrWhiteSpace(version)) {
                version = LatestVersion;
            }

            var basePath = Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.GetAssemblyPath());
            var dockerTempaltePath = Path.Combine(basePath, "DockerTemplate\\DockerfileTemplate");
            var dockerTemplateContent = File.ReadAllText(dockerTempaltePath);
            var dockerImageContent = string.Format(dockerTemplateContent, version, username, password);

            return CreateLocalDockerFromContentAsync(dockerImageContent, name, version, port, cancellationToken);
        }

        public async Task<IContainer> CreateLocalDockerFromFileAsync(string name, string filePath, int port, CancellationToken cancellationToken = default(CancellationToken)) {
            var dockerImageContent = await LoadDockerImageContent(filePath, cancellationToken);
            return await CreateLocalDockerFromContentAsync(dockerImageContent, name, LatestVersion, port, cancellationToken);
        }

        private static async Task<string> LoadDockerImageContent(string filePath, CancellationToken cancellationToken) {
            using (var client = new HttpClient()) {
                using (var result = await client.GetAsync(filePath, cancellationToken)) {
                    if (result.IsSuccessStatusCode) {
                        return await result.Content.ReadAsStringAsync();
                    }

                    throw new FileNotFoundException();
                }
            }
        }

        private async Task<IContainer> CreateLocalDockerFromContentAsync(string dockerImageContent, string name, string tag, int port, CancellationToken cancellationToken) {
            var guid = dockerImageContent.ToGuid().ToString();
            var folder = Path.Combine(Path.GetTempPath(), guid);
            var filePath = Path.Combine(folder, "Dockerfile");

            if (!_fileSystem.DirectoryExists(folder)) {
                _fileSystem.CreateDirectory(folder);
                _fileSystem.WriteAllText(filePath, dockerImageContent);
            }

            try {
                return await _containerService.CreateContainerFromFileAsync(new BuildImageParameters(filePath, guid, tag, name, port), cancellationToken);
            } finally {
                await UpdateContainersOnceAsync(cancellationToken);
            }
        }

        public void Restart() {
            var cts = new CancellationTokenSource();
            var oldCts = Interlocked.Exchange(ref _updateContainersCts, cts);
            oldCts.Cancel();
            if (_containersChangedCountdown.Count > 0) {
                UpdateContainersAsync(cts.Token).DoNotWait();
            }
        }

        public void Dispose() => _updateContainersCts.Cancel();

        private void StartUpdatingContainers() => UpdateContainersAsync(_updateContainersCts.Token).DoNotWait();
        private void EndUpdatingContainers() => Interlocked.Exchange(ref _updateContainersCts, new CancellationTokenSource()).Cancel();

        private async Task UpdateContainersAsync(CancellationToken updateContainersCancellationToken) {
            Error = null;
            Status = ContainersStatus.Running;
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
            } catch (ContainerServiceNotInstalledException) {
                EndUpdatingContainers();
                Status = ContainersStatus.NotInstalled;
            } catch (ContainerServiceNotRunningException) {
                EndUpdatingContainers();
                Status = ContainersStatus.Stopped;
            } catch (ContainerException ex) {
                var message = ex.Message;
                if (message.EqualsOrdinal(Error)) {
                    EndUpdatingContainers();
                    Status = ContainersStatus.HasErrors;
                } else {
                    Error = ex.Message;
                }
            } catch (OperationCanceledException) {
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