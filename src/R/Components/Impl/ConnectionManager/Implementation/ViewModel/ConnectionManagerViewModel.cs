// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf;
using Microsoft.Common.Wpf.Collections;
using Microsoft.R.Components.ConnectionManager.ViewModel;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Components.ConnectionManager.Implementation.ViewModel {
    internal sealed class ConnectionManagerViewModel : BindableBase, IConnectionManagerViewModel {
        private readonly IConnectionManager _connectionManager;
        private readonly ICoreShell _shell;
        private readonly BatchObservableCollection<IConnectionViewModel> _localConnections;
        private readonly BatchObservableCollection<IConnectionViewModel> _remoteConnections;
        private readonly DisposableBag _disposableBag;
        private IConnectionViewModel _editedConnection;
        private bool _isEditingNew;
        private bool _hasLocalConnections;
        private bool _isConnected;

        public ConnectionManagerViewModel(IConnectionManager connectionManager, ICoreShell shell) {
            _connectionManager = connectionManager;
            _shell = shell;
            _disposableBag = DisposableBag.Create<ConnectionManagerViewModel>()
                .Add(() => connectionManager.ConnectionStateChanged -= ConnectionStateChanged);

            _remoteConnections = new BatchObservableCollection<IConnectionViewModel>();
            RemoteConnections = new ReadOnlyObservableCollection<IConnectionViewModel>(_remoteConnections);

            _localConnections = new BatchObservableCollection<IConnectionViewModel>();
            LocalConnections = new ReadOnlyObservableCollection<IConnectionViewModel>(_localConnections);

            connectionManager.ConnectionStateChanged += ConnectionStateChanged;
            IsConnected = connectionManager.IsConnected;
            UpdateConnections();
        }

        public void Dispose() {
            _disposableBag.TryDispose();
        }

        public ReadOnlyObservableCollection<IConnectionViewModel> LocalConnections { get; }
        public ReadOnlyObservableCollection<IConnectionViewModel> RemoteConnections { get; }

        public IConnectionViewModel EditedConnection {
            get { return _editedConnection; }
            private set { SetProperty(ref _editedConnection, value); }
        }

        public bool IsEditingNew {
            get { return _isEditingNew; }
            private set { SetProperty(ref _isEditingNew, value); }
        }

        public bool HasLocalConnections {
            get { return _hasLocalConnections; }
            private set { SetProperty(ref _hasLocalConnections, value); }
        }

        public bool IsConnected {
            get { return _isConnected; }
            private set { SetProperty(ref _isConnected, value); }
        }

        private bool TryStartEditing(IConnectionViewModel connection) {
            _shell.AssertIsOnMainThread();

            // When 'Edit' button is clicked second time, we close the panel.
            // If panel has changes, offer save the changes. 
            if (EditedConnection != null && EditedConnection.HasChanges) {
                var dialogResult = _shell.ShowMessage(Resources.ConnectionManager_EditedConnectionHasChanges, MessageButtons.YesNoCancel);
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

        public void EditNew() {
            _shell.AssertIsOnMainThread();
            IsEditingNew = TryStartEditing(new ConnectionViewModel());
        }

        public void CancelEdit() {
            _shell.AssertIsOnMainThread();
            EditedConnection?.Reset();
            EditedConnection = null;
            IsEditingNew = false;
        }

        public void BrowseLocalPath(IConnectionViewModel connection) {
            _shell.AssertIsOnMainThread();
            string latestLocalPath;
            Uri latestLocalPathUri;

            if (connection.Path != null && Uri.TryCreate(connection.Path, UriKind.Absolute, out latestLocalPathUri) && latestLocalPathUri.IsFile && !latestLocalPathUri.IsUnc) {
                latestLocalPath = latestLocalPathUri.LocalPath;
            } else {
                latestLocalPath = Environment.SystemDirectory;

                try {
                    latestLocalPath = new RInstallation().GetCompatibleEngines().FirstOrDefault()?.InstallPath;
                    if (string.IsNullOrEmpty(latestLocalPath) || !Directory.Exists(latestLocalPath)) {
                        // Force 64-bit PF
                        latestLocalPath = Environment.GetEnvironmentVariable("ProgramW6432");
                    }
                } catch (ArgumentException) { } catch (IOException) { }
            }

            var path = _shell.FileDialog.ShowBrowseDirectoryDialog(latestLocalPath);
            if (path != null) {
                // Verify path
                var ri = new RInterpreterInfo(string.Empty, path);
                if (ri.VerifyInstallation(null, null, _shell)) {
                    connection.Path = path;
                }
            }
        }

        public void Edit(IConnectionViewModel connection) {
            _shell.AssertIsOnMainThread();
            TryStartEditing(connection);
        }

        public void CancelTestConnection(IConnectionViewModel connection) {
            _shell.AssertIsOnMainThread();
            connection.TestingConnectionCts?.Cancel();
            connection.TestingConnectionCts = null;
            connection.IsTestConnectionSucceeded = false;
            connection.TestConnectionFailedText = null;
        }

        public async Task TestConnectionAsync(IConnectionViewModel connection) {
            _shell.AssertIsOnMainThread();
            CancelTestConnection(connection);

            connection.TestingConnectionCts = new CancellationTokenSource();

            try {
                await _connectionManager.TestConnectionAsync(connection, connection.TestingConnectionCts.Token);
                connection.IsTestConnectionSucceeded = true;
            } catch (ArgumentException) {
                if (connection.TestingConnectionCts != null) {
                    connection.TestConnectionFailedText = Resources.ConnectionManager_TestConnectionFailed_PathIsInvalid;
                }
            } catch (RHostDisconnectedException exception) {
                if (connection.TestingConnectionCts != null) {
                    connection.TestConnectionFailedText = Resources.ConnectionManager_TestConnectionFailed_Format.FormatInvariant(exception.Message);
                }
            } catch (RHostBrokerBinaryMissingException) {
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
            }
        }

        public void Save(IConnectionViewModel connectionViewModel) {
            _shell.AssertIsOnMainThread();

            var connection = _connectionManager.AddOrUpdateConnection(
                connectionViewModel.Name,
                connectionViewModel.Path,
                connectionViewModel.RCommandLineArguments,
                connectionViewModel.IsUserCreated);

            if (connection.Id != connectionViewModel.Id && connectionViewModel.Id != null) {
                _connectionManager.TryRemove(connectionViewModel.Id);
            }

            EditedConnection = null;
            IsEditingNew = false;
            UpdateConnections();
        }

        public bool TryDelete(IConnectionViewModel connection) {
            _shell.AssertIsOnMainThread();
            var result = _connectionManager.TryRemove(connection.Id);
            UpdateConnections();
            return result;
        }

        public void Connect(IConnectionViewModel connection) {
            _shell.AssertIsOnMainThread();
            if (connection.IsActive && !IsConnected) {
                _shell.ProgressDialog.Show(_connectionManager.ReconnectAsync, Resources.ConnectionManager_ReconnectionToProgressBarMessage.FormatInvariant(connection.Name));
            } else {
                var progressBarMessage = _connectionManager.ActiveConnection != null
                    ? Resources.ConnectionManager_SwitchConnectionProgressBarMessage.FormatInvariant(_connectionManager.ActiveConnection.Name, connection.Name)
                    : Resources.ConnectionManager_ConnectionToProgressBarMessage.FormatInvariant(connection.Name);
                _shell.ProgressDialog.Show(ct => _connectionManager.ConnectAsync(connection, ct), progressBarMessage);
            }

            UpdateConnections();
        }

        private void UpdateConnections() {
            var selectedId = EditedConnection?.Id;

            _localConnections.ReplaceWith(_connectionManager.RecentConnections
                .Where(c => !c.IsRemote)
                .Select(CreateConnectionViewModel)
                .OrderBy(c => c.Name));

            _remoteConnections.ReplaceWith(_connectionManager.RecentConnections
                .Where(c => c.IsRemote)
                .Select(CreateConnectionViewModel)
                .OrderBy(c => c.Name));

            var editedConnection = RemoteConnections.FirstOrDefault(i => i.Id == selectedId);
            if (editedConnection != null) {
                EditedConnection = editedConnection;
            }

            HasLocalConnections = _localConnections.Count > 0;
        }

        private ConnectionViewModel CreateConnectionViewModel(IConnection connection) {
            var isActive = connection == _connectionManager.ActiveConnection;
            return new ConnectionViewModel(connection, _shell.Services.ColorService) {
                IsActive = isActive,
                IsConnected = isActive && IsConnected
            };
        }

        private void ConnectionStateChanged(object sender, ConnectionEventArgs e) {
            _shell.DispatchOnUIThread(() => {
                IsConnected = e.State;
                UpdateConnections();
            });
        }
    }
}