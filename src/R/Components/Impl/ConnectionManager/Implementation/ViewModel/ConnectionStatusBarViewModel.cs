// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf;
using Microsoft.R.Components.ConnectionManager.ViewModel;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.ConnectionManager.Implementation.ViewModel {
    internal class ConnectionStatusBarViewModel : BindableBase, IConnectionStatusBarViewModel {
        private readonly IConnectionManager _connectionManager;
        private readonly ICoreShell _shell;
        private readonly DisposableBag _disposableBag;

        private bool _isConnected;
        private string _selectedConnection;
        
        public ConnectionStatusBarViewModel(IConnectionManager connectionManager, ICoreShell shell) {
            _connectionManager = connectionManager;
            _shell = shell;
            _disposableBag = DisposableBag.Create<ConnectionManager>()
                .Add(() => connectionManager.ConnectionStateChanged -= ConnectionStateChanged);

            connectionManager.ConnectionStateChanged += ConnectionStateChanged;
            IsConnected = connectionManager.IsConnected;
            SelectedConnection = "Local R v3.3.3";
        }

        public void Dispose() {
            _disposableBag.TryMarkDisposed();
        }

        public bool IsConnected {
            get { return _isConnected; }
            set { SetProperty(ref _isConnected, value); }
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
                SelectedConnection = e.Connection?.Name;
            });
        }
    }
}
