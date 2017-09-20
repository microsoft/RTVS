// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Components.Containers;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Containers;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Components.ConnectionManager.Implementation {
    internal class ConnectionManager : IConnectionManager {
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;
        private readonly IRSettings _settings;
        private readonly IServiceContainer _services;
        private readonly IActionLog _log;
        private readonly IRSessionProvider _sessionProvider;
        private readonly IContainerManager _containers;
        private readonly DisposableBag _disposableBag;
        private readonly ConcurrentDictionary<string, IConnection> _connections;
        private readonly ISecurityService _securityService;
        private readonly IRInstallationService _installationService;
        private readonly object _syncObj = new object();
        private volatile bool _isFirstConnectionAttempt = true;

        public bool IsConnected { get; private set; }
        public bool IsRunning { get; private set; }
        public IConnection ActiveConnection { get; private set; }
        public ReadOnlyCollection<IConnection> RecentConnections { get; private set; }

        public event EventHandler RecentConnectionsChanged;
        public event EventHandler ConnectionStateChanged;

        public ConnectionManager(IRSettings settings, IRInteractiveWorkflowVisual interactiveWorkflow) {
            _sessionProvider = interactiveWorkflow.RSessions;
            _settings = settings;
            _interactiveWorkflow = interactiveWorkflow;
            _services = interactiveWorkflow.Services;
            _containers = interactiveWorkflow.Containers;
            _securityService = _services.GetService<ISecurityService>();
            _log = _services.GetService<IActionLog>();
            _installationService = _services.GetService<IRInstallationService>();
            _services.GetService<IContainerService>();

            _disposableBag = DisposableBag.Create<ConnectionManager>()
                .Add(() => _sessionProvider.BrokerStateChanged -= BrokerStateChanged)
                .Add(() => _interactiveWorkflow.RSession.Connected -= SessionConnected)
                .Add(() => _interactiveWorkflow.RSession.Disconnected -= SessionDisconnected)
                .Add(() => _interactiveWorkflow.ActiveWindowChanged -= ActiveWindowChanged)
                .Add(_containers.SubscribeOnChanges(ContainersChanged));

            _sessionProvider.BrokerStateChanged += BrokerStateChanged;
            _interactiveWorkflow.RSession.Connected += SessionConnected;
            _interactiveWorkflow.RSession.Disconnected += SessionDisconnected;
            _interactiveWorkflow.ActiveWindowChanged += ActiveWindowChanged;

            // Get initial values
            var connections = CreateConnectionList();
            _connections = new ConcurrentDictionary<string, IConnection>(connections);

            UpdateRecentConnections(save: false);
        }
        
        public void Dispose() {
            _disposableBag.TryDispose();
        }

        public IConnection AddOrUpdateConnection(IConnectionInfo connectionInfo) {
            var newConnection = Connection.Create(_securityService, connectionInfo, _settings.ShowHostLoadMeter);
            var connection = _connections.AddOrUpdate(newConnection.Name, newConnection, (k, v) => UpdateConnectionFactory(v, newConnection));
            UpdateRecentConnections();
            return connection;
        }

        public IConnection GetOrAddConnection(string name, string path, string rCommandLineArguments, bool isUserCreated) {
            var connection = GetConnection(name) 
                ?? _connections.GetOrAdd(name, 
                    Connection.Create(_securityService, name, path, rCommandLineArguments, isUserCreated, _settings.ShowHostLoadMeter));

            UpdateRecentConnections();
            return connection;
        }

        public IConnection GetConnection(string name) => _connections.TryGetValue(name, out IConnection connection) ? connection : null;

        public bool TryRemove(string name) {
            if (name.Equals(ActiveConnection?.Name)) {
                return false;
            }

            var isRemoved = _connections.TryRemove(name, out IConnection connection);
            if (isRemoved) {
                UpdateRecentConnections();
                _securityService.DeleteUserCredentials(connection.BrokerConnectionInfo.CredentialAuthority);
            }

            return isRemoved;
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            await _sessionProvider.RemoveBrokerAsync(cancellationToken);
            ActiveConnection = null;
            SaveActiveConnectionToSettings();
        }

        public Task TestConnectionAsync(IConnectionInfo connection, CancellationToken cancellationToken = default(CancellationToken)) {
            var brokerConnectionInfo = (connection as IConnection)?.BrokerConnectionInfo
                ?? BrokerConnectionInfo.Create(_securityService, connection.Name, connection.Path, connection.RCommandLineArguments, _settings.ShowHostLoadMeter);
            return _sessionProvider.TestBrokerConnectionAsync(connection.Name, brokerConnectionInfo, cancellationToken);
        }

        public async Task ReconnectAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            var connection = ActiveConnection;
            if (connection != null && !_sessionProvider.IsConnected) {
                await _sessionProvider.TrySwitchBrokerAsync(connection.Name, connection.BrokerConnectionInfo, cancellationToken);
            }
        }

        public async Task ConnectAsync(IConnectionInfo connection, CancellationToken cancellationToken = default(CancellationToken)) {
            if (await TrySwitchBrokerAsync(connection, cancellationToken)) {
                await _services.MainThread().SwitchToAsync(cancellationToken);
                var interactiveWindow = await _interactiveWorkflow.GetOrCreateVisualComponentAsync();
                interactiveWindow.Container.Show(focus: false, immediate: false);
            }
        }

        public Task<bool> TryConnectToPreviouslyUsedAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            if (!_isFirstConnectionAttempt) {
                return Task.FromResult(false);
            }

            var connectionInfo = _settings.LastActiveConnection;
            if (connectionInfo != null) {
                var connection = GetOrCreateConnection(connectionInfo);
                if (connection.IsRemote) {
                    return Task.FromResult(false); // Do not restore remote connections automatically
                }
            }

            return !string.IsNullOrEmpty(connectionInfo?.Path)
                ? TrySwitchBrokerAsync(connectionInfo, cancellationToken)
                : Task.FromResult(false);
        }

        private async Task<bool> TrySwitchBrokerAsync(IConnectionInfo info, CancellationToken cancellationToken = default(CancellationToken)) {
            var connection = info as IConnection ?? GetOrCreateConnection(info);
            if (ActiveConnection != null && _sessionProvider.HasBroker && connection.BrokerConnectionInfo == ActiveConnection.BrokerConnectionInfo) {
                return false;
            }

            _isFirstConnectionAttempt = false;
            var brokerSwitched = await _sessionProvider.TrySwitchBrokerAsync(connection.Name, connection.BrokerConnectionInfo, cancellationToken);
            if (brokerSwitched) {
                UpdateActiveConnection(connection);
            }

            return brokerSwitched;
        }

        private IConnection GetOrCreateConnection(IConnectionInfo connectionInfo)
            => _connections.TryGetValue(connectionInfo.Name, out var connection)
               ? connection
               : Connection.Create(_securityService, connectionInfo, _settings.ShowHostLoadMeter);

        private IConnection UpdateConnectionFactory(IConnection oldConnection, IConnection newConnection) {
            if (oldConnection != null && newConnection.Equals(oldConnection)) {
                return oldConnection;
            }

            UpdateActiveConnection();
            return newConnection;
        }

        private Dictionary<string, IConnection> GetConnectionsFromSettings() {
            if (_settings.Connections == null) {
                return new Dictionary<string, IConnection>();
            }

            return _settings.Connections
                .Select(c => (IConnection)Connection.Create(_securityService, c, _settings.ShowHostLoadMeter))
                .ToDictionary(k => k.Name);
        }

        private void SaveConnectionsToSettings() {
            _settings.Connections = RecentConnections
                .Select(c => new ConnectionInfo(c))
                .ToArray();
            _settings.SaveSettingsAsync().DoNotWait();
        }

        private void UpdateRecentConnections(bool save = true) {
            RecentConnections = new ReadOnlyCollection<IConnection>(_connections.Values.OrderByDescending(c => c.LastUsed).ToList());
            if (save) {
                SaveConnectionsToSettings();
            }
            RecentConnectionsChanged?.Invoke(this, new EventArgs());
        }

        private Dictionary<string, IConnection> CreateConnectionList() {
            var connections = GetConnectionsFromSettings();
            var localEngines = _installationService.GetCompatibleEngines().ToList();
            var containers = _containers.GetRunningContainers();
            var connectionsToRemove = new List<string>();

            // Remove missing engines and add engines missing from saved connections
            // Set 'is used created' to false if path points to locally found interpreter

            foreach (var connection in connections.Values) {
                var name = connection.Name;

                // Remove connections for missing engines
                if (!connection.IsRemote) {
                    var isValid = connection.Uri.IsFile
                        ? IsValidLocalConnection(name, connection.Path)
                        : containers.Any(c => c.Name.EqualsOrdinal(name));

                    if (!isValid) {
                        connectionsToRemove.Add(name);
                    }
                }

                // For MRS and MRC always use generated name. These upgrade in place
                // so keeping old name with old version doesn't make sense.
                // TODO: handle this better in the future by storing version.
                if (connection.Path.ContainsIgnoreCase("R_SERVER")) {
                    connectionsToRemove.Add(name);
                }
            }

            foreach (var connectionName in connectionsToRemove) {
                connections.Remove(connectionName);
            }

            // Add newly installed engines
            foreach (var e in localEngines) {
                if (!connections.Values.Any(x => x.Path.PathEquals(e.InstallPath))) {
                    connections[e.Name] = Connection.Create(_securityService, e.Name, e.InstallPath, string.Empty, false, _settings.ShowHostLoadMeter);
                }
            }

            // Add running containers
            foreach (var container in containers) {
                if (!connections.ContainsKey(container.Name)) {
                    connections[container.Name] = Connection.Create(_securityService, container, string.Empty, false, _settings.ShowHostLoadMeter);
                }
            }

            // Verify that most recently used connection is still valid
            var last =  _settings.LastActiveConnection;
            if (last != null && connections.TryGetValue(last.Name, out IConnection lastConnection) && !lastConnection.IsRemote && !IsValidLocalConnection(last.Name, last.Path)) {
                // Installation was removed or otherwise disappeared
                _settings.LastActiveConnection = null;
            }

            if (_settings.LastActiveConnection == null && connections.Any()) {
                // Perhaps first time launch with R preinstalled.
                var connection = PickBestLocalRConnection(connections.Values, localEngines);
                connection.LastUsed = DateTime.Now;
                _settings.LastActiveConnection = new ConnectionInfo(connection) {
                    LastUsed = DateTime.Now
                };
            }

            return connections;
        }

        private bool IsValidLocalConnection(string name, string path) {
            try {
                var info = _installationService.CreateInfo(name, path);
                return info.VerifyInstallation();
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                _log.Write(LogVerbosity.Normal, MessageCategory.Error, ex.Message);
            }
            return false;
        }

        private void BrokerStateChanged(object sender, BrokerStateChangedEventArgs eventArgs) {
            lock (_syncObj) {
                IsConnected = _sessionProvider.IsConnected;
                IsRunning &= IsConnected;
            }
            UpdateActiveConnection();
            ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SessionConnected(object sender, EventArgs args) {
            lock (_syncObj) {
                IsConnected = _sessionProvider.IsConnected;
                IsRunning = true;
            }
            ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SessionDisconnected(object sender, EventArgs args) {
            lock (_syncObj) {
                IsConnected = _sessionProvider.IsConnected;
                IsRunning = false;
            }
            ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ActiveWindowChanged(object sender, ActiveWindowChangedEventArgs eventArgs) {
            IsConnected = _sessionProvider.IsConnected && eventArgs.Window != null;
            IsRunning &= IsConnected;
            UpdateActiveConnection();
            ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ContainersChanged() {
            var activeConnection = ActiveConnection;
            foreach (var container in _containers.GetContainers()) {
                if (container.IsRunning) {
                    _connections.GetOrAdd(container.Name, Connection.Create(_securityService, container, string.Empty, false, _settings.ShowHostLoadMeter));
                } else {
                    _connections.TryRemove(container.Name, out IConnection _);
                    if (activeConnection != null && container.Name.EqualsOrdinal(activeConnection.Name)) {
                        _sessionProvider.RemoveBrokerAsync().DoNotWait();
                    }
                }
            }

            UpdateRecentConnections();
        }

        private void UpdateActiveConnection(IConnection candidateConnection = null) {
            lock (_syncObj) {
                var brokerConnectionInfo = _sessionProvider.Broker.ConnectionInfo;
                if (candidateConnection != null) {
                    if (candidateConnection == ActiveConnection && candidateConnection.BrokerConnectionInfo == brokerConnectionInfo) {
                        return;
                    }
                } else if (_sessionProvider.IsConnected && brokerConnectionInfo == (ActiveConnection?.BrokerConnectionInfo ?? default(BrokerConnectionInfo))) {
                    return;
                }

                var connection = candidateConnection ?? RecentConnections.FirstOrDefault(c => brokerConnectionInfo == c.BrokerConnectionInfo);
                if (connection != null) {
                    connection.LastUsed = DateTime.Now;
                }

                ActiveConnection = connection;
                SaveActiveConnectionToSettings();
                UpdateRecentConnections();
            }
        }

        private void SaveActiveConnectionToSettings() {
            _services.MainThread().Post(() => _settings.LastActiveConnection = ActiveConnection == null
                ? null
                : new ConnectionInfo(ActiveConnection));
        }

        private IConnection PickBestLocalRConnection(ICollection<IConnection> connections, ICollection<IRInterpreterInfo> localEngines) {
            // Get highest version engine
            IRInterpreterInfo rInfo = null;
            if (localEngines.Any()) {
                var highest = localEngines.Max(e => e.Version);
                rInfo = localEngines.First(e => e.Version == highest);
            }

            if (rInfo != null) {
                // Find connection matching the highest version
                var c = connections.FirstOrDefault(e => e.Path.PathEquals(rInfo.InstallPath));
                if (c != null) {
                    return c;
                }
            }

            // Nothing found or incompatible. Try first user connection in the list, if any
            return connections.FirstOrDefault();
        }
    }
}