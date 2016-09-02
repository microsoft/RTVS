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
        private readonly DisposableBag _disposableBag;
        private readonly ConnectionStatusBarViewModel _statusBarViewModel;
        private readonly ConcurrentDictionary<Uri, IConnection> _userConnections;

        public bool IsConnected { get; private set; }
        public IConnection ActiveConnection { get; private set; }
        public ReadOnlyCollection<IConnection> RecentConnections { get; private set; }
        public IConnectionManagerVisualComponent VisualComponent { get; private set; }

        public event EventHandler RecentConnectionsChanged;
        public event EventHandler<ConnectionEventArgs> ConnectionStateChanged;

        public ConnectionManager(IStatusBar statusBar, IRSettings settings, IRInteractiveWorkflow interactiveWorkflow) {
            _statusBar = statusBar;
            _sessionProvider = interactiveWorkflow.RSessions;
            _settings = settings;
            _shell = interactiveWorkflow.Shell;

            _statusBarViewModel = new ConnectionStatusBarViewModel(this, interactiveWorkflow.Shell);

            _disposableBag = DisposableBag.Create<ConnectionManager>()
                .Add(_statusBarViewModel)
                .Add(() => _sessionProvider.BrokerChanged -= BrokerChanged)
                .Add(() => interactiveWorkflow.RSession.Connected -= RSessionOnConnected)
                .Add(() => interactiveWorkflow.RSession.Disconnected -= RSessionOnDisconnected);

            _sessionProvider.BrokerChanged += BrokerChanged;
            // TODO: Temporary solution - need to separate RHost errors and network connection issues
            interactiveWorkflow.RSession.Connected += RSessionOnConnected;
            interactiveWorkflow.RSession.Disconnected += RSessionOnDisconnected;

            _shell.DispatchOnUIThread(() => _disposableBag.Add(_statusBar.AddItem(new ConnectionStatusBar { DataContext = _statusBarViewModel })));

            // Get initial values
            var userConnections = GetConnectionsFromSettings();
            _userConnections = new ConcurrentDictionary<Uri, IConnection>(userConnections);

            UpdateRecentConnections();
            SwitchBrokerToLastConnection();
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
            var connection = _userConnections.AddOrUpdate(newConnection.Id, newConnection, (k, v) => UpdateConnectionFactory(v, newConnection));

            UpdateRecentConnections();
            return connection;
        }

        public IConnection GetOrAddConnection(string name, string path, string rCommandLineArguments) {
            var newConnection = CreateConnection(name, path, rCommandLineArguments);
            var connection = _userConnections.GetOrAdd(newConnection.Id, newConnection);
            UpdateRecentConnections();
            return connection;
        }

        public bool TryRemove(Uri id) {
            IConnection connection;
            var isRemoved = _userConnections.TryRemove(id, out connection);
            if (isRemoved) {
                UpdateRecentConnections();
            }

            return isRemoved;
        }

        public async Task ConnectAsync(string name, string path, string rCommandLineArguments) {
            var connection = GetOrCreateConnection(name, path, rCommandLineArguments);
            await ConnectAsync(connection);
        }

        public async Task ConnectAsync(IConnection connection) {
            var sessions = _sessionProvider.GetSessions().ToList();
            if (sessions.Any()) {
                await Task.WhenAll(sessions.Select(s => s.StopHostAsync()));
            }

            if (ActiveConnection != null && ActiveConnection.Id != connection.Id) {
                SwitchBroker(connection);
            }

            if (sessions.Any()) {
                await Task.WhenAll(sessions.Select(s => s.RestartHostAsync()));
            }
        }

        public void SwitchBroker(string name, string path, string rCommandLineArguments) {
            var connection = GetOrCreateConnection(name, path, rCommandLineArguments);
            SwitchBroker(connection);
        }

        private void SwitchBroker(IConnection connection) {
            ActiveConnection = connection;
            SaveActiveConnectionToSettings();
            _sessionProvider.TrySwitchBroker(connection.Name, connection.Path);
        }

        private IConnection CreateConnection(string name, string path, string rCommandLineArguments) => 
            new Connection(name, path, rCommandLineArguments, DateTime.Now);

        private IConnection GetOrCreateConnection(string name, string path, string rCommandLineArguments) {
            var newConnection = CreateConnection(name, path, rCommandLineArguments);
            IConnection connection;
            return _userConnections.TryGetValue(newConnection.Id, out connection) ? connection : newConnection;
        }

        private IConnection UpdateConnectionFactory(IConnection oldConnection, IConnection newConnection) {
            if (oldConnection != null && newConnection.Equals(oldConnection)) {
                return oldConnection;
            }

            UpdateActiveConnection();
            return newConnection;
        }

        private Dictionary<Uri, IConnection> GetConnectionsFromSettings() => _settings.Connections
            .Select(c => CreateConnection(c.Name, c.Path, c.RCommandLineArguments))
            .ToDictionary(k => k.Id);

        private void SaveConnectionsToSettings() {
            _settings.Connections = RecentConnections
                .Select(c => new ConnectionInfo { Name = c.Name, Path = c.Path, RCommandLineArguments = c.RCommandLineArguments })
                .ToArray();
        }

        private void UpdateRecentConnections() {
            RecentConnections = new ReadOnlyCollection<IConnection>(_userConnections.Values.OrderByDescending(c => c.TimeStamp).ToList());
            SaveConnectionsToSettings();
            RecentConnectionsChanged?.Invoke(this, new EventArgs());
        }

        private void SwitchBrokerToLastConnection() {
            var connectionInfo = _settings.LastActiveConnection;
            if (!string.IsNullOrEmpty(connectionInfo?.Path)) {
                SwitchBroker(connectionInfo.Name, connectionInfo.Path, connectionInfo.RCommandLineArguments);
                return;
            }

            var connection = RecentConnections.FirstOrDefault();
            if (connectionInfo != null) {
                SwitchBroker(connection);
                return;
            }

            var localRPath = new RInstallation().GetRInstallPath();
            if (localRPath != null) {
                SwitchBroker(CreateConnection("Local", localRPath, string.Empty));
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
            if (ActiveConnection?.Id == _sessionProvider.BrokerUri) {
                return;
            }

            ActiveConnection = RecentConnections.FirstOrDefault(c => c.Id == _sessionProvider.BrokerUri);
            SaveActiveConnectionToSettings();
        }

        private void SaveActiveConnectionToSettings() {
            _settings.LastActiveConnection = ActiveConnection == null
                ? null
                : new ConnectionInfo {
                    Name = ActiveConnection.Name,
                    Path = ActiveConnection.Path,
                    RCommandLineArguments = ActiveConnection.RCommandLineArguments
                };
        }
    }
}