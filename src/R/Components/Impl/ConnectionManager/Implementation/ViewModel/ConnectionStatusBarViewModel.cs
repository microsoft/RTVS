// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ConnectionManager.ViewModel;

namespace Microsoft.R.Components.ConnectionManager.Implementation.ViewModel {
    internal class ConnectionStatusBarViewModel : ConnectionStatusBaseViewModel, IConnectionStatusBarViewModel {
        private string _selectedConnection;

        public ConnectionStatusBarViewModel(IConnectionManager connectionManager, ICoreShell shell): 
            base(connectionManager, shell) {
            SelectedConnection = connectionManager.ActiveConnection?.Name;
        }

        public string SelectedConnection {
            get { return _selectedConnection; }
            set { SetProperty(ref _selectedConnection, value); }
        }

        public void ShowContextMenu(Point point) {
            Shell.ShowContextMenu(ConnectionManagerCommandIds.ContextMenu, (int)point.X, (int)point.Y);
        }

        protected override void ConnectionStateChanged() {
            SelectedConnection = ConnectionManager.ActiveConnection?.Name;
        }
    }
}
