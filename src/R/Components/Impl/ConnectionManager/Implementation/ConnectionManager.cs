// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ConnectionManager.Implementation.View;
using Microsoft.R.Components.ConnectionManager.Implementation.ViewModel;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.StatusBar;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.R.Components.ConnectionManager.Implementation {
    internal class ConnectionManager : IConnectionManager {
        private readonly IRSettings _settings;
        private readonly ICoreShell _shell;
        private readonly IStatusBar _statusBar;
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRHostBrokerConnector _brokerConnector;
        private readonly DisposableBag _disposableBag;
        private readonly ConnectionStatusBarViewModel _statusBarViewModel;
        private readonly ConcurrentDictionary<string, IConnection> _connections;

        public bool IsConnected { get; private set; }
        public IConnection ActiveConnection { get; private set; }
        public ReadOnlyCollection<IConnection> RecentConnections { get; private set; }

        public event EventHandler RecentConnectionsChanged;
        public event EventHandler<ConnectionEventArgs> ConnectionStateChanged;

        public ConnectionManager(IStatusBar statusBar, IRSessionProvider sessionProvider, IRSettings settings, IRInteractiveWorkflow interactiveWorkflow) {
            _statusBar = statusBar;
            _sessionProvider = sessionProvider;
            _brokerConnector = interactiveWorkflow.BrokerConnector;
            _settings = settings;
            _shell = interactiveWorkflow.Shell;
            _connections = new ConcurrentDictionary<string, IConnection>();

            _statusBarViewModel = new ConnectionStatusBarViewModel(this, interactiveWorkflow.Shell);

            _disposableBag = DisposableBag.Create<ConnectionManager>()
                .Add(_statusBarViewModel)
                .Add(() => _brokerConnector.BrokerChanged -= BrokerChanged)
                .Add(() => interactiveWorkflow.RSession.Connected -= RSessionOnConnected)
                .Add(() => interactiveWorkflow.RSession.Disconnected -= RSessionOnDisconnected);

            _brokerConnector.BrokerChanged += BrokerChanged;
            // TODO: Temporary solution - need to separate RHost errors and network connection issues
            interactiveWorkflow.RSession.Connected += RSessionOnConnected;
            interactiveWorkflow.RSession.Disconnected += RSessionOnDisconnected;

            _shell.DispatchOnUIThread(() => _disposableBag.Add(_statusBar.AddItem(new ConnectionStatusBar { DataContext = _statusBarViewModel })));

            // Set initial value
            AddOrUpdateLocalConnection(_settings.RBasePath, _settings.RBasePath);
            UpdateActiveConnection();
        }

        public void Dispose() {
            _disposableBag.TryMarkDisposed();
        }

        public void AddOrUpdateLocalConnection(string name, string rBasePath) {
            var connection = new LocalConnection(name, rBasePath, DateTime.Now, _sessionProvider, _brokerConnector);
            _connections.AddOrUpdate(name, connection, (k, v) => UpdateConnectionFactory(v, connection));

            RecentConnections = new ReadOnlyCollection<IConnection>(_connections.Values.OrderByDescending(c => c.TimeStamp).ToList());
            RecentConnectionsChanged?.Invoke(this, new EventArgs());
        }

        private IConnection UpdateConnectionFactory(IConnection oldConnection, IConnection newConnection) {
            if (oldConnection != null && newConnection.Equals(oldConnection)) {
                return oldConnection;
            }

            UpdateActiveConnection();
            return newConnection;
        }
        
        private void BrokerChanged(object sender, EventArgs eventArgs) {
            UpdateActiveConnection();
        }

        private void RSessionOnConnected(object sender, RConnectedEventArgs e) {
            IsConnected = true;
            ConnectionStateChanged?.Invoke(this, new ConnectionEventArgs(true, ActiveConnection));
        }

        private void RSessionOnDisconnected(object sender, EventArgs e) {
            IsConnected = false;
            ConnectionStateChanged?.Invoke(this, new ConnectionEventArgs(false, ActiveConnection));
        }

        private void UpdateActiveConnection() {
            ActiveConnection = RecentConnections.FirstOrDefault(c => c.Id == _brokerConnector.BrokerUri);
        }
    }
}