// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Logging;
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
            var userConnections = CreateConnectionList();
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

        public IConnection AddOrUpdateConnection(string name, string path, string rCommandLineArguments, bool isUserCreated) {
            var newConnection = new Connection(name, path, rCommandLineArguments, DateTime.Now, isUserCreated);
            var connection = _userConnections.AddOrUpdate(newConnection.Id, newConnection, (k, v) => UpdateConnectionFactory(v, newConnection));

            UpdateRecentConnections();
            return connection;
        }

        public IConnection GetOrAddConnection(string name, string path, string rCommandLineArguments, bool isUserCreated) {
            var newConnection = CreateConnection(name, path, rCommandLineArguments, isUserCreated);
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

        public async Task ConnectAsync(IConnectionInfo connection) {
            var sessions = _sessionProvider.GetSessions().ToList();
            if (sessions.Any()) {
                await Task.WhenAll(sessions.Select(s => s.StopHostAsync()));
            }

            if (ActiveConnection != null && (!ActiveConnection.Path.EqualsIgnoreCase(connection.Path) || _sessionProvider.BrokerUri.IsLoopback)) {
                SwitchBroker(connection);
            }

            if (sessions.Any()) {
                await Task.WhenAll(sessions.Select(s => s.RestartHostAsync()));
            }
        }

        public void SwitchBroker(IConnectionInfo info) {
            var connection = GetOrCreateConnection(info.Name, info.Path, info.RCommandLineArguments, info.IsUserCreated);
            SwitchBroker(connection);
        }

        private void SwitchBroker(IConnection connection) {
            ActiveConnection = connection;
            SaveActiveConnectionToSettings();
            _sessionProvider.TrySwitchBroker(connection.Name, connection.Path);
        }

        private IConnection CreateConnection(string name, string path, string rCommandLineArguments, bool isUserCreated) =>
            new Connection(name, path, rCommandLineArguments, DateTime.Now, isUserCreated);

        private IConnection GetOrCreateConnection(string name, string path, string rCommandLineArguments, bool isUserCreated) {
            var newConnection = CreateConnection(name, path, rCommandLineArguments, isUserCreated);
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
            .Select(c => CreateConnection(c.Name, c.Path, c.RCommandLineArguments, c.IsUserCreated))
            .ToDictionary(k => k.Id);

        private void SaveConnectionsToSettings() {
            _settings.Connections = RecentConnections
                .Select(c => new ConnectionInfo { Name = c.Name, Path = c.Path, RCommandLineArguments = c.RCommandLineArguments, IsUserCreated = c.IsUserCreated })
                .ToArray();
        }

        private void UpdateRecentConnections() {
            RecentConnections = new ReadOnlyCollection<IConnection>(_userConnections.Values.OrderByDescending(c => c.LastUsed).ToList());
            SaveConnectionsToSettings();
            RecentConnectionsChanged?.Invoke(this, new EventArgs());
        }

        private Dictionary<Uri, IConnection> CreateConnectionList() {
            var connections = GetConnectionsFromSettings();
            var localEngines = new RInstallation().GetCompatibleEngines();
            if (connections.Count == 0) {
                if (!localEngines.Any()) {
                    var message = string.Format(CultureInfo.InvariantCulture, Resources.NoLocalR, Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    if (_shell.ShowMessage(message, MessageButtons.YesNo) == MessageButtons.Yes) {
                        var installer = _shell.ExportProvider.GetExportedValue<IMicrosoftRClientInstaller>();
                        installer.LaunchRClientSetup(_shell);
                        return connections;
                    }
                }
                // No connections, may be first use or connections were somehow removed.
                // Add local connections so there is at least something available.
                foreach (var e in localEngines) {
                    var c = CreateConnection(e.Name, e.InstallPath, string.Empty, isUserCreated: false);
                    connections[new Uri(e.InstallPath, UriKind.Absolute)] = c;
                }
            } else {
                // Remove missing engines and add engines missing from saved connections
                // Set 'is used created' to false if path points to locally found interpreter
                foreach (var kvp in connections.Where(c => !c.Value.IsRemote).ToList()) {
                    bool valid = false;
                    try {
                        var info = new RInterpreterInfo(kvp.Value.Name, kvp.Value.Path);
                        valid = info.VerifyInstallation();
                    } catch (Exception ex) when (!ex.IsCriticalException()) {
                        GeneralLog.Write(ex);
                    }
                    if (!valid) {
                        connections.Remove(kvp.Key);
                    }
                }

                // Add newly installed engines
                foreach (var e in localEngines) {
                    if (!connections.Values.Any(x => x.Path.TrimTrailingSlash().EqualsIgnoreCase(e.InstallPath.TrimTrailingSlash()))) {
                        connections[new Uri(e.InstallPath, UriKind.Absolute)] = CreateConnection(e.Name, e.InstallPath, string.Empty, isUserCreated: false);
                    }
                }
            }
            return connections;
        }

        private void SwitchBrokerToLastConnection() {
            var connectionInfo = _settings.LastActiveConnection;
            if (!string.IsNullOrEmpty(connectionInfo?.Path)) {
                SwitchBroker(connectionInfo);
                return;
            }

            var connection = RecentConnections.FirstOrDefault();
            if (connectionInfo != null) {
                SwitchBroker(connection);
                return;
            }

            var local = _userConnections.Values.FirstOrDefault(c => !c.IsRemote);
            if (local != null) {
                SwitchBroker(local);
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