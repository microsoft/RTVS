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
        private readonly BatchObservableCollection<IConnectionViewModel> _items;
        private readonly DisposableBag _disposableBag;
        private IConnectionViewModel _selectedConnection;
        private IConnectionViewModel _newConnection;
        private bool _isConnected;

        public ConnectionManagerViewModel(IConnectionManager connectionManager, ICoreShell shell) {
            _connectionManager = connectionManager;
            _shell = shell;
            _disposableBag = DisposableBag.Create<ConnectionManagerViewModel>()
                .Add(() => connectionManager.ConnectionStateChanged -= ConnectionStateChanged);

            _items = new BatchObservableCollection<IConnectionViewModel>();
            Items = new ReadOnlyObservableCollection<IConnectionViewModel>(_items);
            connectionManager.ConnectionStateChanged += ConnectionStateChanged;
            IsConnected = connectionManager.IsConnected;
            UpdateConnections();
        }

        public void Dispose() {
            _disposableBag.TryMarkDisposed();
        }

        public ReadOnlyObservableCollection<IConnectionViewModel> Items { get; }

        public IConnectionViewModel NewConnection {
            get { return _newConnection; }
            private set { SetProperty(ref _newConnection, value); }
        }

        public IConnectionViewModel SelectedConnection {
            get { return _selectedConnection; }
            private set { SetProperty(ref _selectedConnection, value); }
        }

        public bool IsConnected {
            get { return _isConnected; }
            private set { SetProperty(ref _isConnected, value); }
        }

        public void SelectConnection(IConnectionViewModel connection) {
            _shell.AssertIsOnMainThread();
            if (connection == SelectedConnection) {
                return;
            }

            if (SelectedConnection != null && SelectedConnection.HasChanges) {
                var dialogResult = _shell.ShowMessage(Resources.ConnectionManager_ChangedSelection_HasChanges, MessageButtons.YesNoCancel);
                switch (dialogResult) {
                    case MessageButtons.Yes:
                        Save(SelectedConnection);
                        break;
                    case MessageButtons.No:
                        CancelEdit();
                        break;
                    default:
                        return;
                }
            }

            SelectedConnection = connection;
        }

        public void AddNew() {
            _shell.AssertIsOnMainThread();
            NewConnection = new ConnectionViewModel();
        }

        public void CancelEdit() {
            _shell.AssertIsOnMainThread();
            SelectedConnection?.Reset();
        }

        public void Cancel(IConnectionViewModel connection) {
            _shell.AssertIsOnMainThread();
            if (connection == NewConnection) {
                NewConnection = null;
            } else {
                connection.Reset();
            }
        }

        public void BrowseLocalPath(IConnectionViewModel connection) {
            string latestLocalPath;
            Uri latestLocalPathUri;

            if (connection.Path != null && Uri.TryCreate(connection.Path, UriKind.Absolute, out latestLocalPathUri) && latestLocalPathUri.IsFile && !latestLocalPathUri.IsUnc) {
                latestLocalPath = latestLocalPathUri.LocalPath;
            } else { 
                latestLocalPath = Environment.SystemDirectory;

                try {
                    latestLocalPath = new RInstallation().GetCompatibleEnginePathFromRegistry();
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
            
        }

        public Task TestConnectionAsync(IConnectionViewModel connection) => Task.CompletedTask;

        public void Save(IConnectionViewModel connectionViewModel) {
            _shell.AssertIsOnMainThread();
            if (string.IsNullOrEmpty(connectionViewModel.Name)) {
                _shell.ShowMessage(Resources.ConnectionManager_ShouldHaveName, MessageButtons.OK);
                return;
            }

            if (string.IsNullOrEmpty(connectionViewModel.Path)) {
                _shell.ShowMessage(Resources.ConnectionManager_ShouldHavePath, MessageButtons.OK);
                return;
            }

            var connection = _connectionManager.AddOrUpdateConnection(
                connectionViewModel.Name,
                connectionViewModel.Path,
                connectionViewModel.RCommandLineArguments);

            if (connection.Id != connectionViewModel.Id && connectionViewModel.Id != null) {
                _connectionManager.TryRemove(connectionViewModel.Id);
            }

            if (connectionViewModel == NewConnection) {
                NewConnection = null;
            }

            UpdateConnections();
        }

        public bool TryDelete(IConnectionViewModel connection) {
            var result = _connectionManager.TryRemove(SelectedConnection.Id);
            UpdateConnections();
            return result;
        }

        public async Task ConnectAsync(IConnectionViewModel connection) {
            _shell.AssertIsOnMainThread();
            await _connectionManager.ConnectAsync(connection.Name, connection.Path, connection.RCommandLineArguments);
            UpdateConnections();
        }

        private void UpdateConnections() { 
            var selectedId = SelectedConnection?.Id;
            _items.ReplaceWith(_connectionManager.RecentConnections.Select(c => new ConnectionViewModel(c) {
                IsActive = c == _connectionManager.ActiveConnection,
                IsConnected = c == _connectionManager.ActiveConnection && IsConnected
            }).OrderBy(c => c.Name));

            var selectedConnection = Items.FirstOrDefault(i => i.Id == selectedId);
            if (selectedConnection != null) {
                SelectedConnection = selectedConnection;
            } else if (Items.Count > 0) {
                SelectedConnection = Items[0];
            }
        }

        private void ConnectionStateChanged(object sender, ConnectionEventArgs e) {
            _shell.DispatchOnUIThread(() => {
                IsConnected = e.State;
                foreach (var item in _items) {
                    item.IsConnected = e.State && item.IsActive;
                }
            });
        }
    }
}