// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
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
using Microsoft.R.Components.ConnectionManager.Commands;
using Microsoft.R.Components.Containers;
using Microsoft.R.Components.Settings;
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
        private CreateLocalDockerFromFileViewModel _newLocalDockerFromFile;
        private bool _containerServiceIsNotInstalled;
        private bool _containerServiceIsNotRunning;
        private string _containerServiceError;

        public ReadOnlyObservableCollection<ContainerViewModel> LocalContainers { get; }

        public CreateLocalDockerViewModel NewLocalDocker {
            get => _newLocalDocker;
            private set => SetProperty(ref _newLocalDocker, value);
        }

        public CreateLocalDockerFromFileViewModel NewLocalDockerFromFile {
            get => _newLocalDockerFromFile;
            private set => SetProperty(ref _newLocalDockerFromFile, value);
        }

        public bool ContainerServiceIsNotInstalled {
            get => _containerServiceIsNotInstalled;
            private set => SetProperty(ref _containerServiceIsNotInstalled, value);
        }

        public bool ContainerServiceIsNotRunning {
            get => _containerServiceIsNotRunning;
            private set => SetProperty(ref _containerServiceIsNotRunning, value);
        }

        public string ContainerServiceError {
            get => _containerServiceError;
            private set => SetProperty(ref _containerServiceError, value);
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
        private void ContrainersChangedMainThread() 
            => _localContainers.ReplaceWith(_containers.GetContainers().Select(c => new ContainerViewModel(c)).OrderBy(c => c.Name));

        private void ContainersStatusChanged(object sender, EventArgs eventArgs) => _mainThread.Post(ContainersStatusChangedMainThread);
        private void ContainersStatusChangedMainThread() {
            ContainerServiceIsNotInstalled = _containers.Status == ContainersStatus.NotInstalled;
            ContainerServiceIsNotRunning = _containers.Status == ContainersStatus.Stopped;
            ContainerServiceError = _containers.Status == ContainersStatus.HasErrors ? _containers.Error : null;
        }

        public void Dispose() => _disposable.TryDispose();

        public void ShowCreateLocalDocker() {
            _mainThread.Assert();
            CancelLocalDockerFromFile();
            var (username, password) = _security.ReadUserCredentials(LastLocalDockerCredentials);
            NewLocalDocker = new CreateLocalDockerViewModel(username, password);
        }

        public void ShowLocalDockerFromFile() {
            _mainThread.Assert();
            CancelCreateLocalDocker();
            NewLocalDockerFromFile = new CreateLocalDockerFromFileViewModel();
        }

        public async Task CreateLocalDockerAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            _mainThread.Assert();

            var (name, username, password, version, port) = NewLocalDocker;
            NewLocalDocker = null;

            IContainer container;
            try {
                container = await _containers.CreateLocalDockerAsync(name, username, password.ToUnsecureString(), version, port, cancellationToken);
            } catch (ContainerException) {
                _ui.ShowMessage(Resources.ContainerManager_CreateLocalDocker_CreationError, MessageButtons.OK, MessageType.Error);
                return;
            }

            var securePassword = password;
            _security.SaveUserCredentials(BrokerConnectionInfo.GetCredentialAuthority(name), username, securePassword, true);
            _security.SaveUserCredentials(LastLocalDockerCredentials, username, securePassword, true);

            try {
                await _containers.StartAsync(container.Id, cancellationToken);
            } catch (ContainerException) {
                _ui.ShowMessage(Resources.ContainerManager_StartError_Format.FormatInvariant(container.Name), MessageButtons.OK, MessageType.Error);
            }
        }
         
        public async Task CreateLocalDockerFromFileAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            _mainThread.Assert();

            var (name, filePath, port) = NewLocalDockerFromFile;
            NewLocalDockerFromFile = null;

            IContainer container;
            try {
                container = await _containers.CreateLocalDockerFromFileAsync(name, filePath, port, cancellationToken);
            } catch (ContainerException) {
                _ui.ShowMessage(Resources.ContainerManager_CreateLocalDocker_CreationError, MessageButtons.OK, MessageType.Error);
                return;
            } catch (FileNotFoundException) {
                _ui.ShowMessage(Resources.ContainerManager_CreateLocalDockerFromFile_FileAccessError_Format.FormatInvariant(filePath), MessageButtons.OK, MessageType.Error);
                return;
            } catch (UriFormatException) {
                _ui.ShowMessage(Resources.ContainerManager_CreateLocalDockerFromFile_UriParseError_Format.FormatInvariant(filePath), MessageButtons.OK, MessageType.Error);
                return;
            } catch (HttpRequestException) {
                _ui.ShowMessage(Resources.ContainerManager_CreateLocalDockerFromFile_HttpError_Format.FormatInvariant(filePath), MessageButtons.OK, MessageType.Error);
                return;
            } catch (UnauthorizedAccessException) {
                _ui.ShowMessage(Resources.ContainerManager_CreateLocalDockerFromFile_UnauthorizedAccess_Format.FormatInvariant(filePath), MessageButtons.OK, MessageType.Error);
                return;
            }

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

        public void CancelLocalDockerFromFile() {
            _mainThread.Assert();
            NewLocalDockerFromFile = null;
        }

        public void BrowseDockerTemplate() {
            _mainThread.Assert();
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            NewLocalDockerFromFile.TemplatePath = _services.UI().FileDialog.ShowBrowseDirectoryDialog(folder);
        }

        public async Task StartAsync(ContainerViewModel container, CancellationToken cancellationToken = default(CancellationToken)) {
            if (container == null) {
                return;
            }

            var runningContainer = _containers.GetRunningContainers()
                .FirstOrDefault(c => c.HostPorts.Intersect(container.HostPorts).Any());

            if (runningContainer != null) {
                var message = IsActiveConnectionToContainer(container)
                    ? Resources.ContainerManager_Start_ActivePortIsBusy_Format.FormatInvariant(container.Name, runningContainer.Name)
                    : Resources.ContainerManager_Start_PortIsBusy_Format.FormatInvariant(container.Name, runningContainer.Name);
                var stopRunning = _ui.ShowMessage(message, MessageButtons.YesNo) == MessageButtons.Yes;
                if (stopRunning) {
                    try {
                        await _containers.StopAsync(runningContainer.Id, cancellationToken);
                    } catch (ContainerException) {
                        _ui.ShowMessage(Resources.ContainerManager_StopError_Format.FormatInvariant(container.Name), MessageButtons.OK, MessageType.Error);
                        return;
                    }
                } else {
                    return;
                }
            }

            try {
                await _containers.StartAsync(container.Id, cancellationToken);
            } catch (ContainerException) {
                _ui.ShowMessage(Resources.ContainerManager_StartError_Format.FormatInvariant(container.Name), MessageButtons.OK, MessageType.Error);
            }
        }

        public async Task ConnectToContainerDefaultConnectionAsync(ContainerViewModel container, CancellationToken cancellationToken = default(CancellationToken)) {
            if (!container.IsRunning) {
                await StartAsync(container, cancellationToken);
            }

            var connection = _connections.GetConnection(container.Name);
            if (connection != null) {
                SwitchToConnectionCommand.Connect(_connections, _ui, _services.GetService<IRSettings>(), connection);
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
                var message = IsActiveConnectionToContainer(container)
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

        private bool IsActiveConnectionToContainer(ContainerViewModel container) 
            => _connections.ActiveConnection?.ContainerName?.EqualsOrdinal(container.Name) ?? false;

        public void RefreshDocker() => _containers.Restart();

        public void ShowConnections()
            => _services.GetService<IRInteractiveWorkflowToolWindowService>().Connections().Show(true, true);
    }
}
