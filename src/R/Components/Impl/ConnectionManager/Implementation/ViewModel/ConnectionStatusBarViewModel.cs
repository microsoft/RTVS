// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf;
using Microsoft.R.Components.ConnectionManager.ViewModel;

namespace Microsoft.R.Components.ConnectionManager.Implementation.ViewModel {
    internal class ConnectionStatusBarViewModel : BindableBase, IConnectionStatusBarViewModel {
        private readonly ICoreShell _shell;

        public ConnectionStatusBarViewModel(ICoreShell shell) {
            _shell = shell;
        }

        private bool _isConnected;
        private string _selectedConnection;

        public bool IsConnected {
            get { return _isConnected; }
            set { SetProperty(ref _isConnected, value); }
        }

        public string SelectedConnection {
            get { return _selectedConnection; }
            set { SetProperty(ref _selectedConnection, value); }
        }


        public void ShowContextMenu(Point point) {
            
        }
    }
}
