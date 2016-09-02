// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf;
using Microsoft.Common.Wpf.Collections;
using Microsoft.R.Components.ConnectionManager.ViewModel;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Components.ConnectionManager.Implementation.ViewModel {
    internal sealed class ConnectionManagerViewModel : BindableBase, IConnectionManagerViewModel {
        private readonly IConnectionManager _connectionManager;
        private readonly ICoreShell _shell;
        private readonly BatchObservableCollection<IConnectionViewModel> _localConnections;
        private readonly BatchObservableCollection<IConnectionViewModel> _userConnections;
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

            _userConnections = new BatchObservableCollection<IConnectionViewModel>();
            UserConnections = new ReadOnlyObservableCollection<IConnectionViewModel>(_userConnections);

            _localConnections = new BatchObservableCollection<IConnectionViewModel>();
            LocalConnections = new ReadOnlyObservableCollection<IConnectionViewModel>(_localConnections);

            connectionManager.ConnectionStateChanged += ConnectionStateChanged;
            IsConnected = connectionManager.IsConnected;
            UpdateConnections();
        }

        public void Dispose() {
            _disposableBag.TryMarkDisposed();
        }

        public ReadOnlyObservableCollection<IConnectionViewModel> LocalConnections { get; }
        public ReadOnlyObservableCollection<IConnectionViewModel> UserConnections { get; }

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
            if (connection == EditedConnection) {
                return false;
            }

            if (EditedConnection != null && EditedConnection.HasChanges) {
                var dialogResult = _shell.ShowMessage(Resources.ConnectionManager_EditedConnectionHasChanges, MessageButtons.YesNoCancel);
                switch (dialogResult) {
                    case MessageButtons.Yes:
                        Save(EditedConnection);
                        break;
                    case MessageButtons.No:
                        CancelEdit();
                        break;
                    default:
                        return false;
                }
            } else {
                CancelEdit();
            }

            EditedConnection = connection;
            connection.IsEditing = true;
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
                        latestLocalPath = Environment.GetEnvironmentVariable("ProgramFiles");
                    }
                }
                catch (ArgumentException) { }
                catch (IOException) { }
            }

            var path = _shell.ShowBrowseDirectoryDialog(latestLocalPath);
            if (path != null) {
                connection.Path = path;
            }
        }

        public void Edit(IConnectionViewModel connection) {
            _shell.AssertIsOnMainThread();
            TryStartEditing(connection);
        }

        public Task TestConnectionAsync(IConnectionViewModel connection) => Task.CompletedTask;

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
            var result = _connectionManager.TryRemove(EditedConnection.Id);
            UpdateConnections();
            return result;
        }

        public async Task ConnectAsync(IConnectionViewModel connection) {
            _shell.AssertIsOnMainThread();
            await _connectionManager.ConnectAsync(connection);
            UpdateConnections();
        }

        private void UpdateConnections() { 
            var selectedId = EditedConnection?.Id;

            _localConnections.ReplaceWith(_connectionManager.RecentConnections.Where(c => !c.IsRemote && !c.IsUserCreated).Select(c => new ConnectionViewModel(c, this) {
                IsActive = c == _connectionManager.ActiveConnection,
                IsConnected = c == _connectionManager.ActiveConnection && IsConnected
            }).OrderBy(c => c.Name));

            _userConnections.ReplaceWith(_connectionManager.RecentConnections.Where(c => c.IsUserCreated).Select(c => new ConnectionViewModel(c, this) {
                IsActive = c == _connectionManager.ActiveConnection,
                IsConnected = c == _connectionManager.ActiveConnection && IsConnected
            }).OrderBy(c => c.Name));

            var editedConnection = UserConnections.FirstOrDefault(i => i.Id == selectedId);
            if (editedConnection != null) {
                EditedConnection = editedConnection;
            }

            HasLocalConnections = _localConnections.Count > 0;
        }

        private void ConnectionStateChanged(object sender, ConnectionEventArgs e) {
            _shell.DispatchOnUIThread(() => {
                IsConnected = e.State;
                foreach (var item in _userConnections) {
                    item.IsConnected = e.State && item.IsActive;
                }
            });
        }
    }
}