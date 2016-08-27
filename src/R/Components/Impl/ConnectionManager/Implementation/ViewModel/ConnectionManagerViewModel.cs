// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf;
using Microsoft.Common.Wpf.Collections;
using Microsoft.R.Components.ConnectionManager.ViewModel;
using Microsoft.R.Components.Extensions;

namespace Microsoft.R.Components.ConnectionManager.Implementation.ViewModel {
    internal sealed class ConnectionManagerViewModel : BindableBase, IConnectionManagerViewModel {
        private readonly IConnectionManager _connectionManager;
        private readonly ICoreShell _shell;
        private readonly BatchObservableCollection<IConnectionViewModel> _items;
        private readonly DisposableBag _disposableBag;
        private IConnectionViewModel _selectedConnection;
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
            if (connection == _selectedConnection) {
                return;
            }

            if (SelectedConnection != null && SelectedConnection.HasChanges) {
                var dialogResult = _shell.ShowMessage(Resources.ConnectionManager_ChangedSelection_HasChanges, MessageButtons.YesNoCancel);
                switch (dialogResult) {
                    case MessageButtons.Yes:
                        SaveSelected();
                        break;
                    case MessageButtons.No:
                        CancelSelected();
                        break;
                    default:
                        return;
                }
            }

            SelectedConnection = connection;
        }

        public void AddNew() {
            SelectConnection(new ConnectionViewModel());
        }

        public void CancelSelected() {
            _shell.AssertIsOnMainThread();
            SelectedConnection?.Reset();
        }

        public void SaveSelected() {
            _shell.AssertIsOnMainThread();
            if (string.IsNullOrEmpty(SelectedConnection.Name)) {
                _shell.ShowMessage(Resources.ConnectionManager_ShouldHaveName, MessageButtons.OK);
                return;
            }

            if (string.IsNullOrEmpty(SelectedConnection.Path)) {
                _shell.ShowMessage(Resources.ConnectionManager_ShouldHavePath, MessageButtons.OK);
                return;
            }

            var connection = _connectionManager.AddOrUpdateConnection(
                SelectedConnection.Name,
                SelectedConnection.Path,
                SelectedConnection.RCommandLineArguments);

            if (connection.Id != SelectedConnection.Id && SelectedConnection.Id != null) {
                _connectionManager.TryRemove(SelectedConnection.Id);
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