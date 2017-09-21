// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Wpf.Collections;
using Microsoft.R.Common.Wpf.Controls;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.Containers;
using Microsoft.R.Components.View;
using Microsoft.R.Containers;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.R.Components.ContainerManager.Implementation.ViewModel {
    internal sealed class ContainerManagerViewModel : BindableBase, IDisposable {
        private static readonly string LastLocalDockerCredentials = $"RTVSInternal:{nameof(LastLocalDockerCredentials)}";
        private readonly IServiceContainer _services;
        private readonly IContainerManager _containers;
        private readonly IConnectionManager _connections;
        private readonly BatchObservableCollection<ContainerViewModel> _localContainers;
        private readonly DisposableBag _disposable;
        private readonly IMainThread _mainThread;
        private readonly IUIService _ui;
        private readonly ISecurityService _security;
        private CreateLocalDockerViewModel _newLocalDocker;
        private bool _containerServiceIsNotInstalled;
        private bool _containerServiceIsNotRunning;

        public ReadOnlyObservableCollection<ContainerViewModel> LocalContainers { get; }

        public CreateLocalDockerViewModel NewLocalDocker {
            get => _newLocalDocker;
            private set => SetProperty(ref _newLocalDocker, value);
        }

        public bool ContainerServiceIsNotInstalled {
            get => _containerServiceIsNotInstalled;
            private set => SetProperty(ref _containerServiceIsNotInstalled, value);
        }

        public bool ContainerServiceIsNotRunning {
            get => _containerServiceIsNotRunning;
            private set => SetProperty(ref _containerServiceIsNotRunning, value);
        }

        public ContainerManagerViewModel(IServiceContainer services) {
            _disposable = DisposableBag.Create<ContainerManagerViewModel>();
            _services = services;
            _mainThread = services.MainThread();
            _ui = services.UI();
            _security = services.Security();
            _containers = services.GetService<IContainerManager>();
            _connections = services.GetService<IConnectionManager>();
            _localContainers = new BatchObservableCollection<ContainerViewModel>();
            LocalContainers = new ReadOnlyObservableCollection<ContainerViewModel>(_localContainers);

            _disposable
                .Add(_containers.SubscribeOnChanges(ContrainersChanged))
                .Add(() => _containers.ContainersStatusChanged -= ContainersStatusChanged);

            _containers.ContainersStatusChanged += ContainersStatusChanged;

            ContrainersChangedMainThread();
            ContainersStatusChangedMainThread();
        }

        private void ContrainersChanged() => _mainThread.Post(ContrainersChangedMainThread);
        private void ContrainersChangedMainThread() => _localContainers.ReplaceWith(_containers.GetContainers().Select(c => new ContainerViewModel(c)));

        private void ContainersStatusChanged(object sender, EventArgs eventArgs) => _mainThread.Post(ContainersStatusChangedMainThread);
        private void ContainersStatusChangedMainThread() {
            ContainerServiceIsNotInstalled = _containers.Status == ContainersStatus.NotInstalled;
            ContainerServiceIsNotRunning = _containers.Status == ContainersStatus.Stopped;
        }

        public void Dispose() => _disposable.TryDispose();

        public void ShowCreateLocalDocker() {
            _mainThread.Assert();
            var (username, password) = _security.ReadUserCredentials(LastLocalDockerCredentials);
            NewLocalDocker = new CreateLocalDockerViewModel(username, password);
        }

        public async Task CreateLocalDockerAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            _mainThread.Assert();

            var name = NewLocalDocker.Name;
            var username = NewLocalDocker.Username;
            var password = NewLocalDocker.Password;
            IContainer container;
            try {
                container = await _containers.CreateLocalDockerAsync(name, username, password.ToUnsecureString(), NewLocalDocker.Version, NewLocalDocker.Port, cancellationToken);
            } catch (ContainerException) {
                _ui.ShowMessage(Resources.ContainerManager_CreateLocalDocker_CreationError, MessageButtons.OK, MessageType.Error);
                return;
            }

            NewLocalDocker = null;

            var securePassword = password;
            _security.SaveUserCredentials(BrokerConnectionInfo.GetCredentialAuthority(name), username, securePassword, true);
            _security.SaveUserCredentials(LastLocalDockerCredentials, username, securePassword, true);

            try {
                await _containers.StartAsync(container.Id, cancellationToken);
            } catch (ContainerException) {
                _ui.ShowMessage(Resources.ContainerManager_StartError_Format.FormatInvariant(container.Name), MessageButtons.OK, MessageType.Error);
            }
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

            if (container.IsRunning) {
                var activeConnection = _connections.ActiveConnection;
                var message = activeConnection != null && container.HostPorts.Contains(activeConnection.Uri.Port)
                    ? Resources.ContainerManager_DeleteActiveWarning_Format.FormatInvariant(container.Name)
                    : Resources.ContainerManager_DeleteRunningWarning_Format.FormatInvariant(container.Name);

                if (_ui.ShowMessage(message, MessageButtons.YesNo) == MessageButtons.No) {
                    return;
                }

                try {
                    await _containers.StopAsync(container.Id, cancellationToken);
                } catch (ContainerException) {
                    _ui.ShowMessage(Resources.ContainerManager_StopError_Format.FormatInvariant(container.Name), MessageButtons.OK, MessageType.Error);
                }
            } else {
                var message = Resources.ContainerManager_DeleteWarning_Format.FormatInvariant(container.Name);
                if (_ui.ShowMessage(message, MessageButtons.YesNo) == MessageButtons.No) {
                    return;
                }
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
