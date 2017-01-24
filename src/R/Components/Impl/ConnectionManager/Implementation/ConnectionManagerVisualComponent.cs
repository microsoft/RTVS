// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ConnectionManager.Implementation.View;
using Microsoft.R.Components.ConnectionManager.Implementation.ViewModel;
using Microsoft.R.Components.ConnectionManager.ViewModel;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.View;

namespace Microsoft.R.Components.ConnectionManager.Implementation {
    public class ConnectionManagerVisualComponent : IConnectionManagerVisualComponent {
        private readonly IConnectionManagerViewModel _viewModel;

        public ConnectionManagerVisualComponent(IConnectionManager connectionManager, IVisualComponentContainer<IConnectionManagerVisualComponent> container, IRSettings settings, ICoreShell coreShell) {
            _viewModel = new ConnectionManagerViewModel(connectionManager, settings, coreShell);
            Container = container;
            var control = new ConnectionManagerControl(coreShell) {
                DataContext = _viewModel
            };
            Control = control;
        }

        public void Dispose() {
            _viewModel.Dispose();
        }

        public FrameworkElement Control { get; }
        public IVisualComponentContainer<IVisualComponent> Container { get; }
    }
}
