// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Wpf.Collections;
using Microsoft.R.Common.Wpf.Controls;
using Microsoft.R.Components.Containers;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.View;

namespace Microsoft.R.Components.ContainerManager.Implementation.ViewModel {
    internal sealed class ContainerManagerViewModel : BindableBase, IDisposable {
        private readonly IServiceContainer _services;
        private readonly IContainerManager _containers;
        private readonly BatchObservableCollection<ContainerViewModel> _localContainers;
        private readonly DisposableBag _disposable;
        private readonly IMainThread _mainThread;
        private readonly IRSettings _settings;
        private CreateLocalDockerViewModel _newLocalDocker;

        public ReadOnlyObservableCollection<ContainerViewModel> LocalContainers { get; }

        public CreateLocalDockerViewModel NewLocalDocker {
            get => _newLocalDocker;
            private set => SetProperty(ref _newLocalDocker, value);
        }

        public ContainerManagerViewModel(IServiceContainer services) {
            _disposable = DisposableBag.Create<ContainerManagerViewModel>();
            _services = services;
            _mainThread = services.MainThread();
            _settings = services.GetService<IRSettings>();
            _containers = services.GetService<IContainerManager>();
            _localContainers = new BatchObservableCollection<ContainerViewModel>();
            LocalContainers = new ReadOnlyObservableCollection<ContainerViewModel>(_localContainers);

            _disposable.Add(_containers.SubscribeOnChanges(ContrainersChanged));

            ContrainersChangedMainThread();
        }

        private void ContrainersChanged() => _mainThread.Post(ContrainersChangedMainThread);
        private void ContrainersChangedMainThread() => _localContainers.ReplaceWith(_containers.GetContainers().Select(c => new ContainerViewModel(c)));

        public void Dispose() => _disposable.TryDispose();

        public void ShowCreateLocalDocker() {
            NewLocalDocker = new CreateLocalDockerViewModel();
        }

        public void CreateLocalDocker() {
            NewLocalDocker = null;
        }

        public void CancelCreateLocalDocker() {
            NewLocalDocker = null;
        }

        public Task StartAsync(ContainerViewModel container, CancellationToken cancellationToken = default(CancellationToken)) 
            => container != null ? _containers.StartAsync(container.Id, cancellationToken) : Task.CompletedTask;

        public Task StopAsync(ContainerViewModel container, CancellationToken cancellationToken = default(CancellationToken)) 
            => container != null ? _containers.StopAsync(container.Id, cancellationToken) : Task.CompletedTask;

        public Task DeleteAsync(ContainerViewModel container, CancellationToken cancellationToken = default(CancellationToken)) 
            => container != null ? _containers.DeleteAsync(container.Id, cancellationToken) : Task.CompletedTask;

        public void ShowConnections() 
            => _services.GetService<IRInteractiveWorkflowToolWindowService>().Connections().Show(true, true);
    }
}
