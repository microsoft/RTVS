// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using Microsoft.Common.Core.Imaging;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.ConnectionManager.ViewModel;

namespace Microsoft.R.Components.ConnectionManager.Implementation.ViewModel {
    public class ConnectionStatusBarViewModel : ConnectionStatusBaseViewModel, IConnectionStatusBarViewModel {
        private readonly IImageService _images;

        private string _selectedConnection;
        private object _icon;
        private object _overlayIcon;

        public ConnectionStatusBarViewModel(IServiceContainer services): 
            base(services) {
            SelectedConnection = ConnectionManager.ActiveConnection?.Name;
            _images = services.GetService<IImageService>();
        }

        public string SelectedConnection {
            get => _selectedConnection;
            set => SetProperty(ref _selectedConnection, value);
        }

        public object Icon {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public object OverlayIcon {
            get => _overlayIcon;
            private set => SetProperty(ref _overlayIcon, value);
        }

        public void ShowContextMenu(Point point) 
            => Services.ShowContextMenu(ConnectionManagerCommandIds.ContextMenu, (int)point.X, (int)point.Y);

        protected override void UpdateConnections() {
            var activeConnection = ConnectionManager.ActiveConnection;
            if (activeConnection != null) {
                SelectedConnection = activeConnection.Name;
                Icon = activeConnection.IsRemote ? _images.GetImage("Cloud") : activeConnection.IsContainer ? _images.GetImage("StructurePublic") : _images.GetImage("Computer");
                OverlayIcon = IsRunning ? _images.GetImage("StatusOK") : IsConnected ? _images.GetImage("StatusWarning") : _images.GetImage("StatusError");
            } else {
                SelectedConnection = null;
                Icon = null;
                OverlayIcon = null;
            }
        }
    }
}
