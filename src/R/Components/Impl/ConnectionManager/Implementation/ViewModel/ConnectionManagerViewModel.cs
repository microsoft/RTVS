// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf.Collections;
using Microsoft.R.Components.ConnectionManager.ViewModel;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Components.ConnectionManager.Implementation.ViewModel {
    internal sealed class ConnectionManagerViewModel : ConnectionStatusBaseViewModel, IConnectionManagerViewModel {
        private readonly BatchObservableCollection<IConnectionViewModel> _localConnections;
        private readonly BatchObservableCollection<IConnectionViewModel> _remoteConnections;
        private IConnectionViewModel _editedConnection;
        private IConnectionViewModel _testingConnection;
        private bool _isEditingNew;
        private bool _hasLocalConnections;

        public ConnectionManagerViewModel(IConnectionManager connectionManager, ICoreShell shell): 
            base(connectionManager, shell) {

            _remoteConnections = new BatchObservableCollection<IConnectionViewModel>();
            RemoteConnections = new ReadOnlyObservableCollection<IConnectionViewModel>(_remoteConnections);

            _localConnections = new BatchObservableCollection<IConnectionViewModel>();
            LocalConnections = new ReadOnlyObservableCollection<IConnectionViewModel>(_localConnections);

            IsConnected = connectionManager.IsConnected;
            UpdateConnections();
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

        private bool TryStartEditing(IConnectionViewModel connection) {
            Shell.AssertIsOnMainThread();

            // When 'Edit' button is clicked second time, we close the panel.
            // If panel has changes, offer save the changes. 
            if (EditedConnection != null && EditedConnection.HasChanges) {
                var dialogResult = Shell.ShowMessage(Resources.ConnectionManager_EditedConnectionHasChanges, MessageButtons.YesNoCancel);
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
            Shell.AssertIsOnMainThread();
            IsEditingNew = TryStartEditing(new ConnectionViewModel());
        }

        public void CancelEdit() {
            Shell.AssertIsOnMainThread();
            EditedConnection?.Reset();
            EditedConnection = null;
            IsEditingNew = false;
        }

        public void BrowseLocalPath(IConnectionViewModel connection) {
            Shell.AssertIsOnMainThread();
            if (connection == null) {
                return;    
            }

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

            var path = Shell.FileDialog.ShowBrowseDirectoryDialog(latestLocalPath);
            if (path != null) {
                // Verify path
                var ri = new RInterpreterInfo(string.Empty, path);
                if (ri.VerifyInstallation(null, null, Shell)) {
                    connection.Path = path;
                }
            }
        }

        public void Edit(IConnectionViewModel connection) {
            Shell.AssertIsOnMainThread();
            if (connection == null) {
                return;    
            }

            TryStartEditing(connection);
        }

        public void CancelTestConnection() {
            Shell.AssertIsOnMainThread();
            if (_testingConnection != null) {
                _testingConnection.TestingConnectionCts?.Cancel();
                _testingConnection.TestingConnectionCts = null;
                _testingConnection.IsTestConnectionSucceeded = false;
                _testingConnection.TestConnectionFailedText = null;
            }
        }

        public async Task TestConnectionAsync(IConnectionViewModel connection) {
            Shell.AssertIsOnMainThread();
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
                _testingConnection = null;
            }
        }

        public void Save(IConnectionViewModel connectionViewModel) {
            Shell.AssertIsOnMainThread();
            if (connectionViewModel == null || !connectionViewModel.HasChanges) {
                return;    
            }

            if (connectionViewModel.IsRenamed && ConnectionManager.GetConnection(connectionViewModel.Name) != null) {
                Shell.ShowMessage(Resources.ConnectionManager_CantSaveWithTheSameName.FormatCurrent(connectionViewModel.Name), MessageButtons.OK);
                return;
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
            Shell.AssertIsOnMainThread();
            CancelTestConnection();

            if (connection != null) {
                if (connection.IsActive) {
                    var confirm = Shell.ShowMessage(Resources.ConnectionManager_RemoveActiveConnectionConfirmation.FormatCurrent(connection.Name), MessageButtons.YesNo);
                    if (confirm == MessageButtons.Yes) {
                        Shell.ProgressDialog.Show(ct => ConnectionManager.RemoveAsync(connection.Name, ct), Resources.ConnectionManager_DeleteConnectionProgressBarMessage.FormatInvariant(connection.Name));
                        UpdateConnections();
                        return true;
                    }
                } else {
                    var confirm = Shell.ShowMessage(Resources.ConnectionManager_RemoveConnectionConfirmation.FormatCurrent(connection.Name), MessageButtons.YesNo);
                    if (confirm == MessageButtons.Yes) {
                        var result = ConnectionManager.TryRemove(connection.Name);
                        UpdateConnections();
                        return result;
                    }
                }
            }
            return false;
        }

        public void Connect(IConnectionViewModel connection, bool connectToEdited) {
            Shell.AssertIsOnMainThread();
            if (connection == null) {
                return;    
            }

            if (connection != EditedConnection) {
                CancelEdit();
            } else if (connectToEdited) {
                Save(connection);
            } else {
                return;
            }

            CancelTestConnection();
            
            if (connection.IsActive && !IsConnected) {
                Shell.ProgressDialog.Show(ConnectionManager.ReconnectAsync, Resources.ConnectionManager_ReconnectionToProgressBarMessage.FormatInvariant(connection.Name));
            } else {
                var activeConnection = ConnectionManager.ActiveConnection;
                var connectionToSwitch = ConnectionManager.GetConnection(connection.Name);
                if (activeConnection != null && connectionToSwitch.BrokerConnectionInfo == activeConnection.BrokerConnectionInfo) {
                    var text = Resources.ConnectionManager_ConnectionsAreIdentical.FormatCurrent(activeConnection.Name, connection.Name);
                    Shell.ShowMessage(text, MessageButtons.OK);
                } else {
                    var progressBarMessage = activeConnection != null
                        ? Resources.ConnectionManager_SwitchConnectionProgressBarMessage.FormatInvariant(activeConnection.Name, connection.Name)
                        : Resources.ConnectionManager_ConnectionToProgressBarMessage.FormatInvariant(connection.Name);
                    Shell.ProgressDialog.Show(ct => ConnectionManager.ConnectAsync(connection, ct), progressBarMessage);
                }
            }

            UpdateConnections();
        }

        private void UpdateConnections() {
            var selectedConnectionName = EditedConnection?.Name;

            _localConnections.ReplaceWith(ConnectionManager.RecentConnections
                .Where(c => !c.IsRemote)
                .Select(CreateConnectionViewModel)
                .OrderBy(c => c.Name));

            _remoteConnections.ReplaceWith(ConnectionManager.RecentConnections
                .Where(c => c.IsRemote)
                .Select(CreateConnectionViewModel)
                .OrderBy(c => c.Name));

            var editedConnection = RemoteConnections.FirstOrDefault(i => i.Name == selectedConnectionName);
            if (editedConnection != null) {
                EditedConnection = editedConnection;
            }

            HasLocalConnections = _localConnections.Count > 0;
        }

        private ConnectionViewModel CreateConnectionViewModel(IConnection connection) {
            var isActive = connection == ConnectionManager.ActiveConnection;
            return new ConnectionViewModel(connection) {
                IsActive = isActive,
                IsConnected = isActive && ConnectionManager.IsConnected,
                IsRunning = isActive && ConnectionManager.IsRunning
            };
        }

        protected override void ConnectionStateChanged() {
            UpdateConnections();
        }
    }
}