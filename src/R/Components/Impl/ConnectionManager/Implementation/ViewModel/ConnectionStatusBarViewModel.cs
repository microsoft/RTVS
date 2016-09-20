// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf;
using Microsoft.R.Components.ConnectionManager.ViewModel;

namespace Microsoft.R.Components.ConnectionManager.Implementation.ViewModel {
    internal class ConnectionStatusBarViewModel : BindableBase, IConnectionStatusBarViewModel {
        private readonly IConnectionManager _connectionManager;
        private readonly ICoreShell _shell;
        private readonly DisposableBag _disposableBag;

        private bool _isRemote;
        private bool _isConnected;
        private string _selectedConnection;
        
        public ConnectionStatusBarViewModel(IConnectionManager connectionManager, ICoreShell shell) {
            _connectionManager = connectionManager;
            _shell = shell;
            _disposableBag = DisposableBag.Create<ConnectionStatusBarViewModel>()
                .Add(() => connectionManager.ConnectionStateChanged -= ConnectionStateChanged);

            connectionManager.ConnectionStateChanged += ConnectionStateChanged;
            IsConnected = connectionManager.IsConnected;
            SelectedConnection = connectionManager.ActiveConnection?.Name;
        }

        public void Dispose() {
            _disposableBag.TryDispose();
        }

        public bool IsConnected {
            get { return _isConnected; }
            set { SetProperty(ref _isConnected, value); }
        }

        public bool IsRemote {
            get { return _isRemote; }
            set { SetProperty(ref _isRemote, value); }
        }

        public string SelectedConnection {
            get { return _selectedConnection; }
            set { SetProperty(ref _selectedConnection, value); }
        }
        
        public void ShowContextMenu(Point point) {
            _shell.ShowContextMenu(ConnectionManagerCommandIds.ContextMenu, (int)point.X, (int)point.Y);
        }
        
        private void ConnectionStateChanged(object sender, ConnectionEventArgs e) {
            _shell.DispatchOnUIThread(() => {
                IsConnected = e.State;
                IsRemote = e.Connection?.IsRemote ?? false;
                SelectedConnection = e.Connection?.Name;
            });
        }
    }
}
