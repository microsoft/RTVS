// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Disposables;
using Microsoft.R.Components.ConnectionManager.Implementation.View;
using Microsoft.R.Components.ConnectionManager.Implementation.ViewModel;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.StatusBar;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Install;

namespace Microsoft.R.Components.ConnectionManager.Implementation {
    internal class ConnectionManager : IConnectionManager {
        private readonly IRSettings _settings;
        private readonly IStatusBar _statusBar;
        private readonly DisposableBag _disposableBag;
        private readonly ConnectionStatusBarViewModel _statusBarViewModel;

        public ConnectionManager(IStatusBar statusBar, IRSettings settings, IRInteractiveWorkflow interactiveWorkflow) {
            _statusBar = statusBar;
            _settings = settings;
            var session = interactiveWorkflow.RSession;
            session.Connected += RSessionOnConnected;
            session.Disconnected += RSessionOnDisconnected;

            _statusBarViewModel = new ConnectionStatusBarViewModel(interactiveWorkflow.Shell) {
                IsConnected = session.IsHostRunning,
                SelectedConnection = "Local: Microsoft R Open v3.3.0"
            };

            _disposableBag = DisposableBag.Create<ConnectionManager>()
                .Add(() => session.Connected -= RSessionOnConnected)
                .Add(() => session.Disconnected -= RSessionOnDisconnected);

            interactiveWorkflow.Shell.DispatchOnUIThread(() => _disposableBag.Add(_statusBar.AddItem(new ConnectionStatusBar { DataContext = _statusBarViewModel })));
        }

        private void RSessionOnConnected(object sender, RConnectedEventArgs e) {
            _statusBarViewModel.IsConnected = true;
            // TODO: Use user-defined name instead
            _statusBarViewModel.SelectedConnection = $"Local R v{e.Name}";
        }

        private void RSessionOnDisconnected(object sender, EventArgs e) {
            _statusBarViewModel.IsConnected = false;
        }

        public void Dispose() {
            _disposableBag.TryMarkDisposed();
        }
    }
}