// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Enums;
using Microsoft.Common.Core.Imaging;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Wpf.Collections;
using Microsoft.R.Components.ConnectionManager.ViewModel;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Interpreters;
using Microsoft.VisualStudio.Imaging;

namespace Microsoft.R.Components.ConnectionManager.Implementation.ViewModel {
    internal sealed class ConnectionManagerViewModel : ConnectionStatusBaseViewModel, IConnectionManagerViewModel {
        private readonly IUIService _ui;
        private readonly IImageService _images;
        private readonly IRSettings _settings;
        private readonly IRInstallationService _installationService;
        private readonly BatchObservableCollection<IConnectionViewModel> _localConnections;
        private readonly BatchObservableCollection<IConnectionViewModel> _localDockerConnections;
        private readonly BatchObservableCollection<IConnectionViewModel> _remoteConnections;
        private IConnectionViewModel _editedConnection;
        private IConnectionViewModel _testingConnection;
        private bool _isEditingNew;
        private bool _hasLocalConnections;

        public ConnectionManagerViewModel(IServiceContainer services) :
            base(services) {
            _ui = services.UI();
            _images = services.GetService<IImageService>();
            _settings = services.GetService<IRSettings>();
            _installationService = services.GetService<IRInstallationService>();

            _localConnections = new BatchObservableCollection<IConnectionViewModel>();
            LocalConnections = new ReadOnlyObservableCollection<IConnectionViewModel>(_localConnections);

            _localDockerConnections = new BatchObservableCollection<IConnectionViewModel>();
            LocalDockerConnections = new ReadOnlyObservableCollection<IConnectionViewModel>(_localDockerConnections);

            _remoteConnections = new BatchObservableCollection<IConnectionViewModel>();
            RemoteConnections = new ReadOnlyObservableCollection<IConnectionViewModel>(_remoteConnections);

            IsConnected = ConnectionManager.IsConnected;
            UpdateConnections();
        }

        public ReadOnlyObservableCollection<IConnectionViewModel> LocalConnections { get; }
        public ReadOnlyObservableCollection<IConnectionViewModel> LocalDockerConnections { get; }
        public ReadOnlyObservableCollection<IConnectionViewModel> RemoteConnections { get; }

        public IConnectionViewModel EditedConnection {
            get => _editedConnection;
            private set => SetProperty(ref _editedConnection, value);
        }

        public bool IsEditingNew {
            get => _isEditingNew;
            private set => SetProperty(ref _isEditingNew, value);
        }

        public bool HasLocalConnections {
            get { return _hasLocalConnections; }
            private set { SetProperty(ref _hasLocalConnections, value); }
        }

        private bool TryStartEditing(IConnectionViewModel connection) {
            Services.MainThread().Assert();

            // When 'Edit' button is clicked second time, we close the panel.
            // If panel has changes, offer save the changes. 
            if (EditedConnection != null && EditedConnection.HasChanges) {
                var dialogResult = _ui.ShowMessage(Resources.ConnectionManager_EditedConnectionHasChanges,
                    MessageButtons.YesNoCancel);
                switch (dialogResult) {
                    case MessageButtons.Yes:
                        Save(EditedConnection);
                        break;
                    case MessageButtons.Cancel:
                        return false;
                }
            }

            var wasEditingConnection = EditedConnection;
            CancelEdit();

            // If it is the same connection that was edited then we came here as a result 
            // of a second click on the edit button. Don't start editing it again.
            if (connection != wasEditingConnection) {
                EditedConnection = connection;
                connection.IsEditing = true;
            }
            return true;
        }

        public bool TryEditNew() {
            Services.MainThread().Assert();
            IsEditingNew = TryStartEditing(new ConnectionViewModel(_images));
            return IsEditingNew;
        }

        public void CancelEdit() {
            Services.MainThread().Assert();
            EditedConnection?.Reset();
            EditedConnection = null;
            IsEditingNew = false;
        }

        public void ShowContainers() => Services.GetService<IRInteractiveWorkflowToolWindowService>().Containers().Show(true, true);

        public void BrowseLocalPath(IConnectionViewModel connection) {
            Services.MainThread().Assert();
            if (connection == null) {
                return;
            }

            string latestLocalPath;
            Uri latestLocalPathUri;

            if (connection.Path != null && Uri.TryCreate(connection.Path, UriKind.Absolute, out latestLocalPathUri) &&
                latestLocalPathUri.IsFile && !latestLocalPathUri.IsUnc) {
                latestLocalPath = latestLocalPathUri.LocalPath;
            } else {
                latestLocalPath = Environment.SystemDirectory;

                try {
                    latestLocalPath = _installationService.GetCompatibleEngines().FirstOrDefault()?.InstallPath;
                    if (string.IsNullOrEmpty(latestLocalPath) || !Directory.Exists(latestLocalPath)) {
                        // Force 64-bit PF
                        latestLocalPath = Environment.GetEnvironmentVariable("ProgramW6432");
                    }
                } catch (ArgumentException) { } catch (IOException) { }
            }

            var path = _ui.FileDialog.ShowBrowseDirectoryDialog(latestLocalPath);
            if (path != null) {
                // Verify path
                var ri = _installationService.CreateInfo(string.Empty, path);
                if (ri.VerifyInstallation(null, Services)) {
                    connection.Path = path;
                }
            }
        }

        public bool TryEdit(IConnectionViewModel connection) {
            Services.MainThread().Assert();
            if (connection == null) {
                return false;
            }

            return TryStartEditing(connection);
        }

        public void CancelTestConnection() {
            Services.MainThread().Assert();
            if (_testingConnection != null) {
                _testingConnection.TestingConnectionCts?.Cancel();
                _testingConnection.TestingConnectionCts = null;
                _testingConnection.IsTestConnectionSucceeded = false;
                _testingConnection.TestConnectionFailedText = null;
            }
        }

        public async Task TestConnectionAsync(IConnectionViewModel connection) {
            Services.MainThread().Assert();
            if (connection == null) {
                return;
            }

            CancelTestConnection();

            connection.TestingConnectionCts = new CancellationTokenSource();
            _testingConnection = connection;

            try {
                await ConnectionManager.TestConnectionAsync(connection, connection.TestingConnectionCts.Token);
                connection.IsTestConnectionSucceeded = true;
            } catch (ArgumentException) {
                if (connection.TestingConnectionCts != null) {
                    connection.TestConnectionFailedText = Resources.ConnectionManager_TestConnectionFailed_PathIsInvalid;
                }
            } catch (RHostDisconnectedException exception) {
                if (connection.TestingConnectionCts != null) {
                    connection.TestConnectionFailedText = Resources.ConnectionManager_TestConnectionFailed_Format.FormatInvariant(exception.Message);
                }
            } catch (ComponentBinaryMissingException) {
                if (connection.TestingConnectionCts != null) {
                    connection.TestConnectionFailedText = Resources.ConnectionManager_TestConnectionFailed_RHostIsMissing;
                }
            } catch (OperationCanceledException) {
                if (connection.TestingConnectionCts != null) {
                    connection.TestConnectionFailedText = Resources.ConnectionManager_TestConnectionCanceled;
                }
            } finally {
                connection.TestingConnectionCts?.Dispose();
                connection.TestingConnectionCts = null;
                _testingConnection = null;
            }
        }

        public void Save(IConnectionViewModel connectionViewModel) {
            Services.MainThread().Assert();
            if (connectionViewModel == null || !connectionViewModel.HasChanges) {
                return;
            }

            if ((connectionViewModel.IsRenamed || IsEditingNew) &&
                ConnectionManager.GetConnection(connectionViewModel.Name) != null) {
                _ui.ShowMessage(Resources.ConnectionManager_CantSaveWithTheSameName.FormatCurrent(connectionViewModel.Name), MessageButtons.OK);
                return;
            }

            if (connectionViewModel.IsConnected) {
                var confirm = _ui.ShowMessage(Resources.ConnectionManager_RenameActiveConnectionConfirmation.FormatCurrent(connectionViewModel.OriginalName), MessageButtons.YesNo);
                if (confirm == MessageButtons.Yes) {
                    var message = Resources.ConnectionManager_RenameConnectionProgressBarMessage.FormatInvariant(connectionViewModel.OriginalName, connectionViewModel.Name);
                    try {
                        _ui.ProgressDialog.Show(ct => ConnectionManager.DisconnectAsync(ct), message);
                    } catch (OperationCanceledException) {
                        return;
                    }
                }
            }

            ConnectionManager.AddOrUpdateConnection(connectionViewModel);

            if (connectionViewModel.IsRenamed) {
                ConnectionManager.TryRemove(connectionViewModel.OriginalName);
            }

            EditedConnection = null;
            IsEditingNew = false;
            UpdateConnections();
        }

        public bool TryDelete(IConnectionViewModel connection) {
            Services.MainThread().Assert();
            CancelTestConnection();

            if (connection == null) {
                return false;
            }

            var confirmMessage = connection.IsActive
                ? Resources.ConnectionManager_RemoveActiveConnectionConfirmation.FormatCurrent(connection.Name)
                : Resources.ConnectionManager_RemoveConnectionConfirmation.FormatCurrent(connection.Name);

            var confirm = _ui.ShowMessage(confirmMessage, MessageButtons.YesNo);
            if (confirm == MessageButtons.No) {
                return false;
            }

            if (connection.IsActive) {
                try {
                    _ui.ProgressDialog.Show(ct => ConnectionManager.DisconnectAsync(ct), Resources.ConnectionManager_DeleteConnectionProgressBarMessage.FormatInvariant(connection.Name));
                } catch (OperationCanceledException) {
                    return false;
                }
            }

            var result = ConnectionManager.TryRemove(connection.Name);
            UpdateConnections();
            return result;
        }

        public void Connect(IConnectionViewModel connection, bool connectToEdited) {
            Services.MainThread().Assert();
            if (connection == null || !connection.IsValid) {
                return;
            }

            if (connection != EditedConnection) {
                CancelEdit();
            } else if (connectToEdited) {
                connection.UpdatePath();
                Save(connection);
            } else {
                return;
            }

            CancelTestConnection();

            if (connection.IsActive && !IsConnected) {
                _ui.ProgressDialog.Show(ConnectionManager.ReconnectAsync, Resources.ConnectionManager_ReconnectionToProgressBarMessage.FormatInvariant(connection.Name));
            } else {
                var activeConnection = ConnectionManager.ActiveConnection;
                var connectionToSwitch = ConnectionManager.GetConnection(connection.Name);
                if (activeConnection != null && connectionToSwitch.BrokerConnectionInfo == activeConnection.BrokerConnectionInfo) {
                    var text = Resources.ConnectionManager_ConnectionsAreIdentical.FormatCurrent(activeConnection.Name, connection.Name);
                    _ui.ShowMessage(text, MessageButtons.OK);
                } else {
                    var connect = true;
                    if (activeConnection != null && _settings.ShowWorkspaceSwitchConfirmationDialog == YesNo.Yes) {
                        var message = Resources.ConnectionManager_SwitchConfirmation.FormatCurrent(activeConnection.Name, connection.Name);
                        if (_ui.ShowMessage(message, MessageButtons.YesNo) == MessageButtons.No) {
                            connect = false;
                        }
                    }

                    if (connect) {
                        var progressBarMessage = activeConnection != null
                            ? Resources.ConnectionManager_SwitchConnectionProgressBarMessage.FormatCurrent(activeConnection.Name, connection.Name)
                            : Resources.ConnectionManager_ConnectionToProgressBarMessage.FormatCurrent(connection.Name);
                        _ui.ProgressDialog.Show(ct => ConnectionManager.ConnectAsync(connection, ct), progressBarMessage);
                    }
                }
            }

            UpdateConnections();
        }

        protected override void UpdateConnections() {
            var selectedConnectionName = EditedConnection?.Name;

            using (_localConnections.StartBatchUpdate())
            using (_localDockerConnections.StartBatchUpdate())
            using (_remoteConnections.StartBatchUpdate()) {
                _localConnections.Clear();
                _localDockerConnections.Clear();
                _remoteConnections.Clear();
                foreach (var connection in ConnectionManager.RecentConnections.OrderBy(c => c.Name)) {
                    if (connection.IsRemote) {
                        CreateRemoteConnectionViewModel(connection);
                    } else if (connection.IsContainer) {
                        CreateLocalDockerConnectionViewModel(connection);
                    } else {
                        CreateLocalConnectionViewModel(connection);
                    }
                }
            }

            var editedConnection = RemoteConnections.FirstOrDefault(i => i.Name == selectedConnectionName);
            if (editedConnection != null) {
                EditedConnection = editedConnection;
            }

            HasLocalConnections = _localConnections.Count > 0;
        }

        private void CreateRemoteConnectionViewModel(IConnection connection) {
            var cvm = new ConnectionViewModel(connection, ConnectionManager, _images);
            var commandLine = GetConnectionTooltipCommandLine(connection);

            cvm.ConnectionTooltip = Resources.ConnectionManager_InformationTooltipFormatRemote.FormatInvariant(connection.Path, commandLine);
            cvm.ButtonEditTooltip = Resources.ConnectionManager_EditRemoteTooltip_Format.FormatInvariant(connection.Name);
            cvm.Icon = _images.GetImage("Cloud");
            _remoteConnections.Add(cvm);
        }

        private void CreateLocalDockerConnectionViewModel(IConnection connection) {
            var cvm = new ConnectionViewModel(connection, ConnectionManager, _images);
            var commandLine = GetConnectionTooltipCommandLine(connection);

            cvm.ConnectionTooltip = Resources.ConnectionManager_InformationTooltipFormatLocalDocker.FormatInvariant(connection.Path, connection.ContainerName, commandLine);
            cvm.ButtonEditTooltip = Resources.ConnectionManager_EditLocalDockerTooltip_Format.FormatInvariant(connection.Name);
            cvm.ButtonDeleteDisabledTooltip = Resources.ConnectionManager_DeleteLocalDockerDisabledTooltip;
            cvm.Icon = _images.GetImage("StructurePublic");
            _localDockerConnections.Add(cvm);
        }

        private void CreateLocalConnectionViewModel(IConnection connection) {
            var cvm = new ConnectionViewModel(connection, ConnectionManager, _images);
            var commandLine = GetConnectionTooltipCommandLine(connection);

            cvm.ConnectionTooltip = Resources.ConnectionManager_InformationTooltipFormatLocal.FormatInvariant(connection.Path, commandLine);
            cvm.ButtonEditTooltip = Resources.ConnectionManager_EditLocalTooltip_Format.FormatInvariant(connection.Name);
            cvm.ButtonDeleteDisabledTooltip = Resources.ConnectionManager_DeleteLocalDisabledTooltip;
            cvm.Icon = _images.GetImage("Computer");
            _localConnections.Add(cvm);
        }

        private static string GetConnectionTooltipCommandLine(IConnectionInfo connection) => !string.IsNullOrWhiteSpace(connection.RCommandLineArguments)
            ? connection.RCommandLineArguments
            : Resources.ConnectionManager_None;
    }
}