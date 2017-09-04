// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Containers;

namespace Microsoft.R.Components.Containers.Implementation {
    internal class ContainerManager : IContainerManager {
        private readonly IContainerService _containerService;
        private readonly CountdownDisposable _containersChangedCountdown;
        private ImmutableArray<IContainer> _containers;
        private ImmutableArray<IContainer> _runningContainers;
        private CancellationTokenSource _updateContainersCts;
        private event Action ContainersChanged;

        public ContainerManager(IRInteractiveWorkflow interactiveWorkflow) {
            _containerService = interactiveWorkflow.Services.GetService<IContainerService>();
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

        public void Dispose() => _updateContainersCts.Cancel();

        private void StartUpdatingContainers() => UpdateContainersAsync().DoNotWait();
        private void EndUpdatingContainers() => Interlocked.Exchange(ref _updateContainersCts, new CancellationTokenSource()).Cancel();

        private async Task UpdateContainersAsync() {
            while (!_updateContainersCts.Token.IsCancellationRequested) {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(_updateContainersCts.Token);
                cts.CancelAfter(5000);
                try {
                    UpdateContainers(await _containerService.ListContainersAsync(true, cts.Token));
                } catch (OperationCanceledException) when (!_updateContainersCts.Token.IsCancellationRequested) {
                } catch (ContainerException) {
                    _containersChangedCountdown.Reset();
                }

                await Task.Delay(10000, _updateContainersCts.Token);
            }
        }

        private void UpdateContainers(IEnumerable<IContainer> newContainers) {
            var containers = newContainers.ToImmutableArray();
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