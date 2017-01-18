// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components;
using Microsoft.R.Components.ConnectionManager.Implementation.View;
using Microsoft.R.Components.ConnectionManager.Implementation.ViewModel;
using Microsoft.R.Components.Information;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.StatusBar;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Components.ConnectionManager.Implementation {
    internal class ConnectionManager : IConnectionManager {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly IRSettings _settings;
        private readonly ICoreShell _shell;
        private readonly IStatusBar _statusBar;
        private readonly IRSessionProvider _sessionProvider;
        private readonly DisposableBag _disposableBag;
        private readonly ConnectionStatusBarViewModel _statusBarViewModel;
        private readonly HostLoadIndicatorViewModel _hostLoadIndicatorViewModel;
        private readonly ConcurrentDictionary<string, IConnection> _connections;
        private readonly ISecurityService _securityService;
        private readonly object _syncObj = new object();
        private volatile bool _isFirstConnectionAttempt = true;

        public bool IsConnected { get; private set; }
        public bool IsRunning { get; private set; }
        public IConnection ActiveConnection { get; private set; }
        public ReadOnlyCollection<IConnection> RecentConnections { get; private set; }
        public IConnectionManagerVisualComponent VisualComponent { get; private set; }

        public event EventHandler RecentConnectionsChanged;
        public event EventHandler ConnectionStateChanged;

        public ConnectionManager(IStatusBar statusBar, IRSettings settings, IRInteractiveWorkflow interactiveWorkflow) {
            _statusBar = statusBar;
            _sessionProvider = interactiveWorkflow.RSessions;
            _settings = settings;
            _interactiveWorkflow = interactiveWorkflow;
            _shell = interactiveWorkflow.Shell;
            _securityService = _shell.Services.Security;

            _statusBarViewModel = new ConnectionStatusBarViewModel(this, interactiveWorkflow.Shell);
            _hostLoadIndicatorViewModel = new HostLoadIndicatorViewModel(_sessionProvider, interactiveWorkflow.Shell);

            _disposableBag = DisposableBag.Create<ConnectionManager>()
                .Add(_statusBarViewModel)
                .Add(_hostLoadIndicatorViewModel)
                .Add(() => _sessionProvider.BrokerStateChanged -= BrokerStateChanged)
                .Add(() => _interactiveWorkflow.RSession.Connected -= SessionConnected)
                .Add(() => _interactiveWorkflow.RSession.Disconnected -= SessionDisconnected)
                .Add(() => _interactiveWorkflow.ActiveWindowChanged -= ActiveWindowChanged);

            _sessionProvider.BrokerStateChanged += BrokerStateChanged;

            _interactiveWorkflow.RSession.Connected += SessionConnected;
            _interactiveWorkflow.RSession.Disconnected += SessionDisconnected;
            _interactiveWorkflow.ActiveWindowChanged += ActiveWindowChanged;

            // Get initial values
            var connections = CreateConnectionList();
            _connections = new ConcurrentDictionary<string, IConnection>(connections);

            UpdateRecentConnections(save: false);
            CompleteInitializationAsync().DoNotWait();
        }

        private async Task CompleteInitializationAsync() {
            await _shell.SwitchToMainThreadAsync();
            AddToStatusBar(new ConnectionStatusBar(), _statusBarViewModel);
            AddToStatusBar(new HostLoadIndicator(), _hostLoadIndicatorViewModel);
        }

        private void AddToStatusBar(FrameworkElement element, object dataContext) {
            element.DataContext = dataContext;
            _disposableBag.Add(_statusBar.AddItem(element));
        }

        public void Dispose() {
            _disposableBag.TryDispose();
        }

        public IConnectionManagerVisualComponent GetOrCreateVisualComponent(int instanceId = 0) {
            if (VisualComponent != null) {
                return VisualComponent;
            }

            var visualComponentContainerFactory = _shell.ExportProvider.GetExportedValue<IConnectionManagerVisualComponentContainerFactory>();
            VisualComponent = visualComponentContainerFactory.GetOrCreate(this, instanceId).Component;
            return VisualComponent;
        }

        public IConnection AddOrUpdateConnection(IConnectionInfo connectionInfo) {
            var newConnection = new Connection(connectionInfo);
            var connection = _connections.AddOrUpdate(newConnection.Name, newConnection, (k, v) => UpdateConnectionFactory(v, newConnection));
            UpdateRecentConnections();
            return connection;
        }

        public IConnection GetOrAddConnection(string name, string path, string rCommandLineArguments, bool isUserCreated) {
            var connection = GetConnection(name) ?? _connections.GetOrAdd(name, new Connection(name, path, rCommandLineArguments, isUserCreated));
            UpdateRecentConnections();
            return connection;
        }

        public IConnection GetConnection(string name) {
            IConnection connection;
            return _connections.TryGetValue(name, out connection) ? connection : null;
        }

        public bool TryRemove(string name) {
            if (name.Equals(ActiveConnection?.Name)) {
                return false;
            }

            IConnection connection;
            var isRemoved = _connections.TryRemove(name, out connection);
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
            var brokerConnectionInfo = (connection as IConnection)?.BrokerConnectionInfo ?? BrokerConnectionInfo.Create(connection.Name, connection.Path, connection.RCommandLineArguments);
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
                await _shell.SwitchToMainThreadAsync(cancellationToken);
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

        private IConnection GetOrCreateConnection(IConnectionInfo connectionInfo) {
            IConnection connection;
            return _connections.TryGetValue(connectionInfo.Name, out connection) ? connection : new Connection(connectionInfo);
        }

        private IConnection UpdateConnectionFactory(IConnection oldConnection, IConnection newConnection) {
            if (oldConnection != null && newConnection.Equals(oldConnection)) {
                return oldConnection;
            }

            UpdateActiveConnection();
            return newConnection;
        }

        private Dictionary<string, IConnection> GetConnectionsFromSettings() {
            if(_settings.Connections == null) {
                return new Dictionary<string, IConnection>();
            }

            return _settings.Connections
                .Select(c => (IConnection)new Connection(c))
                .ToDictionary(k => k.Name);
        }

        private void SaveConnectionsToSettings() {
            _settings.Connections = RecentConnections
                .Select(c => new ConnectionInfo (c))
                .ToArray();
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
            var localEngines = new RInstallation().GetCompatibleEngines().ToList();

            // Remove missing engines and add engines missing from saved connections
            // Set 'is used created' to false if path points to locally found interpreter
            foreach (var kvp in connections.Where(c => !c.Value.IsRemote).ToList()) {
                var valid = IsValidLocalConnection(kvp.Value.Name, kvp.Value.Path);
                if (!valid) {
                    connections.Remove(kvp.Key);
                }
            }

            // Add newly installed engines
            foreach (var e in localEngines) {
                if (!connections.Values.Any(x => x.Path.PathEquals(e.InstallPath))) {
                    connections[e.Name] = new Connection(e.Name, e.InstallPath, string.Empty, isUserCreated: false);
                }
            }

            // Verify that most recently used connection is still valid
            var last = _settings.LastActiveConnection;
            if (last != null && !IsRemoteConnection(last.Path) && !IsValidLocalConnection(last.Name, last.Path)) {
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
                var info = new RInterpreterInfo(name, path);
                return info.VerifyInstallation();
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                _shell.Services.Log.Write(LogVerbosity.Normal, MessageCategory.Error, ex.Message);
            }
            return false;
        }

        private bool IsRemoteConnection(string path) {
            try {
                Uri uri;
                return Uri.TryCreate(path, UriKind.Absolute, out uri) && !uri.IsFile;
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                _shell.Services.Log.Write(LogVerbosity.Normal, MessageCategory.Error, ex.Message);
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

        public static ConcurrentQueue<string> Events = new ConcurrentQueue<string>();

        private void ActiveWindowChanged(object sender, ActiveWindowChangedEventArgs eventArgs) {
            IsConnected = _sessionProvider.IsConnected && eventArgs.Window != null;
            IsRunning &= IsConnected;
            UpdateActiveConnection();
            ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateActiveConnection(IConnection candidateConnection = null) {
            lock (_syncObj) {
                var brokerConnectionInfo = _sessionProvider.Broker.ConnectionInfo;
                if (candidateConnection != null) {
                    if (candidateConnection == ActiveConnection && candidateConnection.BrokerConnectionInfo == brokerConnectionInfo) {
                        return;
                    }
                } else if (brokerConnectionInfo == (ActiveConnection?.BrokerConnectionInfo ?? default(BrokerConnectionInfo))) {
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
            _shell.DispatchOnUIThread(() => _settings.LastActiveConnection = ActiveConnection == null
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