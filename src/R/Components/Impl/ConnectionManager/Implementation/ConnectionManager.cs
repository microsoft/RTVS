// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ConnectionManager.Implementation.View;
using Microsoft.R.Components.ConnectionManager.Implementation.ViewModel;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.StatusBar;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Components.ConnectionManager.Implementation {
    internal class ConnectionManager : IConnectionManager {
        private readonly IRSettings _settings;
        private readonly ICoreShell _shell;
        private readonly IStatusBar _statusBar;
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRHostBrokerConnector _brokerConnector;
        private readonly DisposableBag _disposableBag;
        private readonly ConnectionStatusBarViewModel _statusBarViewModel;
        private readonly ConcurrentDictionary<Uri, IConnection> _connections;

        public bool IsConnected { get; private set; }
        public IConnection ActiveConnection { get; private set; }
        public ReadOnlyCollection<IConnection> RecentConnections { get; private set; }
        public IConnectionManagerVisualComponent VisualComponent { get; private set; }

        public event EventHandler RecentConnectionsChanged;
        public event EventHandler<ConnectionEventArgs> ConnectionStateChanged;

        public ConnectionManager(IStatusBar statusBar, IRSessionProvider sessionProvider, IRSettings settings, IRInteractiveWorkflow interactiveWorkflow) {
            _statusBar = statusBar;
            _sessionProvider = sessionProvider;
            _brokerConnector = interactiveWorkflow.BrokerConnector;
            _settings = settings;
            _shell = interactiveWorkflow.Shell;

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

            // Get initial values
            var connections = GetConnectionsFromSettings();
            _connections = new ConcurrentDictionary<Uri, IConnection>(connections);

            UpdateRecentConnections();
            SwitchBrokerToMostRecent();
        }

        private Dictionary<Uri, IConnection> GetConnectionsFromSettings() => _settings.Connections
            .Select(c => CreateConnection(c.Name, c.Path, c.RCommandLineArguments))
            .ToDictionary(k => k.Id);

        private void SaveConnectionsToSettings() {
            _settings.Connections = RecentConnections
                .Select(c => new ConnectionInfo { Name = c.Name, Path = c.Path, RCommandLineArguments = c.RCommandLineArguments })
                .ToArray();
        }

        public void Dispose() {
            _disposableBag.TryMarkDisposed();
        }

        public IConnectionManagerVisualComponent GetOrCreateVisualComponent(IConnectionManagerVisualComponentContainerFactory visualComponentContainerFactory, int instanceId = 0) {
            if (VisualComponent != null) {
                return VisualComponent;
            }

            VisualComponent = visualComponentContainerFactory.GetOrCreate(this, instanceId).Component;
            return VisualComponent;
        }

        public IConnection AddOrUpdateConnection(string name, string path, string rCommandLineArguments) {
            var newConnection = new Connection(name, path, rCommandLineArguments, DateTime.Now);
            var connection = _connections.AddOrUpdate(newConnection.Id, newConnection, (k, v) => UpdateConnectionFactory(v, newConnection));

            UpdateRecentConnections();
            return connection;
        }

        public IConnection GetOrAddConnection(string name, string path, string rCommandLineArguments) {
            var newConnection = CreateConnection(name, path, rCommandLineArguments);
            var connection = _connections.GetOrAdd(newConnection.Id, newConnection);
            UpdateRecentConnections();
            return connection;
        }

        public bool TryRemove(Uri id) {
            IConnection connection;
            var isRemoved = _connections.TryRemove(id, out connection);
            if (isRemoved) {
                UpdateRecentConnections();
            }

            return isRemoved;
        }

        public async Task ConnectAsync(string name, string path, string rCommandLineArguments) {
            var newConnection = CreateConnection(name, path, rCommandLineArguments);
            IConnection connection;
            if (_connections.TryGetValue(newConnection.Id, out connection)) {
                await ConnectAsync(connection);
            } else {
                await ConnectAsync(newConnection);
            }
        }

        public async Task ConnectAsync(IConnection connection) {
            var sessionsToRestart = _sessionProvider.GetSessions()
                .Where(s => s.IsHostRunning)
                .ToList();
            SwitchBroker(connection);
            if (sessionsToRestart.Count > 0) {
                var sessionRestartTasks = sessionsToRestart.Select(s => s.RestartHostAsync());
                await Task.WhenAll(sessionRestartTasks);
            }
        }
        
        private void SwitchBroker(IConnection connection) {
            ActiveConnection = connection;
            if (connection.IsRemote) {
                _brokerConnector.SwitchToRemoteBroker(connection.Id, connection.RCommandLineArguments);
            } else {
                _brokerConnector.SwitchToLocalBroker(connection.Name, connection.Path, connection.RCommandLineArguments);
            }
        }

        private IConnection CreateConnection(string name, string uri, string rCommandLineArguments) => 
            new Connection(name, uri, rCommandLineArguments, DateTime.Now);

        private IConnection UpdateConnectionFactory(IConnection oldConnection, IConnection newConnection) {
            if (oldConnection != null && newConnection.Equals(oldConnection)) {
                return oldConnection;
            }

            UpdateActiveConnection();
            return newConnection;
        }

        private void UpdateRecentConnections() {
            RecentConnections = new ReadOnlyCollection<IConnection>(_connections.Values.OrderByDescending(c => c.TimeStamp).ToList());
            SaveConnectionsToSettings();
            RecentConnectionsChanged?.Invoke(this, new EventArgs());
        }

        private void SwitchBrokerToMostRecent() {
            var connection = RecentConnections.FirstOrDefault();
            if (connection != null) {
                SwitchBroker(connection);
            } else {
                var localRPath = new RInstallation().GetRInstallPath();
                if (localRPath != null) {
                    SwitchBroker(CreateConnection("Local", localRPath, string.Empty));
                }
            }
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
            if (ActiveConnection?.Id == _brokerConnector.BrokerUri) {
                return;
            }

            ActiveConnection = RecentConnections.FirstOrDefault(c => c.Id == _brokerConnector.BrokerUri);
        }
    }
}