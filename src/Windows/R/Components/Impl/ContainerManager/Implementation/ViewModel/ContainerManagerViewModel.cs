// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Wpf.Collections;
using Microsoft.R.Common.Wpf.Controls;
using Microsoft.R.Components.Containers;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.View;
using Microsoft.R.Containers;

namespace Microsoft.R.Components.ContainerManager.Implementation.ViewModel {
    internal sealed class ContainerManagerViewModel : BindableBase, IDisposable {
        private readonly IServiceContainer _services;
        private readonly IContainerManager _containers;
        private readonly BatchObservableCollection<ContainerViewModel> _localContainers;
        private readonly DisposableBag _disposable;
        private readonly IMainThread _mainThread;
        private readonly IUIService _ui;
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
            _ui = services.UI();
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
            _mainThread.Assert();
            NewLocalDocker = new CreateLocalDockerViewModel {
                Username = _settings.LastLocalDockerUsername,
                Password = _settings.LastLocalDockerPassword
            };
        }

        public async Task CreateLocalDockerAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            _mainThread.Assert();

            IContainer container;
            try {
                container = await _containers.CreateLocalDockerAsync(NewLocalDocker.Name, NewLocalDocker.Username, NewLocalDocker.Password, NewLocalDocker.Version, cancellationToken);
            } catch (ContainerException) {
                _ui.ShowMessage(Resources.ContainerManager_CreateLocalDocker_CreationError, MessageButtons.OK, MessageType.Error);
                return;
            }

            _settings.LastLocalDockerUsername = NewLocalDocker.Username;
            _settings.LastLocalDockerPassword = NewLocalDocker.Password;

            try {
                await _containers.StartAsync(container.Id, cancellationToken);
            } catch (ContainerException) {
                _ui.ShowMessage(Resources.ContainerManager_StartError_Format.FormatInvariant(container.Name), MessageButtons.OK, MessageType.Error);
                return;
            }

            NewLocalDocker = null;
        }
         
        public void CancelCreateLocalDocker() {
            _mainThread.Assert();
            NewLocalDocker = null;
        }

        public async Task StartAsync(ContainerViewModel container, CancellationToken cancellationToken = default(CancellationToken)) {
            if (container == null) {
                return;
            }

            try {
                await _containers.StartAsync(container.Id, cancellationToken);
            } catch (ContainerException) {
                _ui.ShowMessage(Resources.ContainerManager_StartError_Format.FormatInvariant(container.Name), MessageButtons.OK, MessageType.Error);
            }
        }

        public async Task StopAsync(ContainerViewModel container, CancellationToken cancellationToken = default(CancellationToken)) {
            if (container == null) {
                return;
            }

            try {
                await _containers.StopAsync(container.Id, cancellationToken);
            } catch (ContainerException) {
                _ui.ShowMessage(Resources.ContainerManager_StopError_Format.FormatInvariant(container.Name), MessageButtons.OK, MessageType.Error);
            }
        }

        public async Task DeleteAsync(ContainerViewModel container, CancellationToken cancellationToken = default(CancellationToken)) {
            if (container == null) {
                return;
            }

            try {
                await _containers.DeleteAsync(container.Id, cancellationToken);
            } catch (ContainerException) {
                _ui.ShowMessage(Resources.ContainerManager_DeleteError_Format.FormatInvariant(container.Name), MessageButtons.OK, MessageType.Error);
            }
        }

        public void ShowConnections() 
            => _services.GetService<IRInteractiveWorkflowToolWindowService>().Connections().Show(true, true);
    }
}
