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
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Telemetry;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Containers;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Components.Containers.Implementation {
    internal class ContainerManager : IContainerManager {
        private const string RtvsImageName = "rtvs-image";
        private const string LatestVersion = "latest";
        private const string TelemetryEvent_BeginCreatingPredefined = "Begin creating predefined";
        private const string TelemetryEvent_EndCreatingPredefined = "End creating predefined";
        private const string TelemetryEvent_BeginCreatingFromFile = "Begin creating from file";
        private const string TelemetryEvent_EndCreatingFromFile = "End creating from file";
        private const string TelemetryEvent_Delete = "End creating from file";

        private readonly IContainerService _containerService;
        private readonly ITelemetryService _telemetry;
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
            _telemetry = interactiveWorkflow.Services.Telemetry();
            _updateContainersCts = new CancellationTokenSource();
            _containers = ImmutableArray<IContainer>.Empty;
            _runningContainers = ImmutableArray<IContainer>.Empty;
            _containersChangedCountdown = new CountdownDisposable(StartUpdatingContainers, EndUpdatingContainers);

            Status = ContainersStatus.Running;
        }

        public IReadOnlyList<IContainer> GetContainers() => _containers;
        public IReadOnlyList<IContainer> GetRunningContainers() => _runningContainers;

        public async Task<IEnumerable<string>> GetLocalDockerVersions(CancellationToken cancellationToken = default (CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();
            const string imageName = "microsoft/rtvs"; // change to microsoft/rtvs;
            try {
                return await GetVersionsFromDockerHubAsync(imageName, cancellationToken);
            } catch (Exception ex) when (!(ex is OperationCanceledException)) {
                try {
                    return await GetVersionsFromLocalImagesAsync(imageName, cancellationToken);
                } catch (ContainerException) {
                    return Enumerable.Empty<string>();
                }
            }
        }

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
                _telemetry.ReportEvent(TelemetryArea.Containers, TelemetryEvent_Delete);
                await _containerService.DeleteContainerAsync(containerId, cancellationToken);
            } finally {
                await UpdateContainersOnceAsync(cancellationToken);
            }
        }

        public Task<IContainer> CreateLocalDockerAsync(string name, string username, string password, string version, int port, CancellationToken cancellationToken = default(CancellationToken)) {
            if (string.IsNullOrWhiteSpace(version)) {
                version = LatestVersion;
            }

            _telemetry.ReportEvent(TelemetryArea.Containers, TelemetryEvent_BeginCreatingPredefined, version);
            var basePath = Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.GetAssemblyPath());
            var dockerfilePath = Path.Combine(basePath, "DockerTemplate\\Dockerfile");

            var hash = $"{username}|{password}".ToGuid().ToString("N");
            var parameters = new BuildImageParameters(dockerfilePath, $"{RtvsImageName}{hash}", version, new Dictionary<string, string> {
                ["RTVS_VERSION"] = version,
                ["USERNAME"] = username,
                ["PASSWORD"] = password
            }, name, port);
            var container = CreateLocalDockerFromContentAsync(parameters, cancellationToken);
            _telemetry.ReportEvent(TelemetryArea.Containers, TelemetryEvent_EndCreatingPredefined, version);

            return container;
        }

        public Task<IContainer> CreateLocalDockerFromFileAsync(string name, string filePath, int port, CancellationToken cancellationToken = default(CancellationToken)) {
            _telemetry.ReportEvent(TelemetryArea.Containers, TelemetryEvent_BeginCreatingFromFile);
            var container = CreateLocalDockerFromContentAsync(new BuildImageParameters(filePath, Guid.NewGuid().ToString(), LatestVersion, name, port), cancellationToken);
            _telemetry.ReportEvent(TelemetryArea.Containers, TelemetryEvent_EndCreatingFromFile);
            return container;
        }

        private async Task<IContainer> CreateLocalDockerFromContentAsync(BuildImageParameters buildOptions, CancellationToken cancellationToken) {
            try {
                return await _containerService.CreateContainerFromFileAsync(buildOptions, cancellationToken);
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
        
        private static async Task<IEnumerable<string>> GetVersionsFromDockerHubAsync(string imageName, CancellationToken cancellationToken) {
            using (var client = new HttpClient()) {
                var uri = new Uri($"https://registry.hub.docker.com/v1/repositories/{imageName}/tags", UriKind.Absolute);
                using (var response = await client.GetAsync(uri, cancellationToken)) {
                    if (!response.IsSuccessStatusCode) {
                        throw new HttpRequestException();
                    }

                    var result = await response.Content.ReadAsStringAsync();
                    return JArray.Parse(result)
                        .OfType<JObject>()
                        .Select(o => o.GetValue("name"))
                        .OfType<JValue>()
                        .Select(v => (string) v.Value)
                        .Where(s => !s.EndsWith("-base"));
                }
            }
        }

        private async Task<IEnumerable<string>> GetVersionsFromLocalImagesAsync(string imageName, CancellationToken cancellationToken) {
            var images = await _containerService.ListImagesAsync(true, cancellationToken);
            return images
                .Where(i => i.Name.EqualsIgnoreCase(imageName))
                .Select(i => i.Tag);
        }

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