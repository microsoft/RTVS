// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ConnectionManager.Implementation.View;
using Microsoft.R.Components.ConnectionManager.Implementation.ViewModel;
using Microsoft.R.Components.Information;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.StatusBar;
using Microsoft.R.Host.Client;
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
        private readonly ConcurrentDictionary<string, IConnection> _userConnections;
        private readonly ISecurityService _securityService;

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
            _interactiveWorkflow = interactiveWorkflow;
            _shell = interactiveWorkflow.Shell;
            _securityService = _shell.ExportProvider.GetExportedValue<ISecurityService>();

            _statusBarViewModel = new ConnectionStatusBarViewModel(this, interactiveWorkflow.Shell);
            _hostLoadIndicatorViewModel = new HostLoadIndicatorViewModel(_sessionProvider, interactiveWorkflow.Shell);

            _disposableBag = DisposableBag.Create<ConnectionManager>()
                .Add(_statusBarViewModel)
                .Add(_hostLoadIndicatorViewModel)
                .Add(() => _sessionProvider.BrokerStateChanged -= BrokerStateChanged)
                .Add(() => _interactiveWorkflow.ActiveWindowChanged -= ActiveWindowChanged);

            _sessionProvider.BrokerStateChanged += BrokerStateChanged;
            _interactiveWorkflow.ActiveWindowChanged += ActiveWindowChanged;

            // Get initial values
            var userConnections = CreateConnectionList();
            _userConnections = new ConcurrentDictionary<string, IConnection>(userConnections);

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

        public IConnection AddOrUpdateConnection(string name, string path, string rCommandLineArguments, bool isUserCreated) {
            var newConnection = CreateConnection(name, path, rCommandLineArguments, isUserCreated);
            var connection = _userConnections.AddOrUpdate(newConnection.Name, newConnection, (k, v) => UpdateConnectionFactory(v, newConnection));
            UpdateRecentConnections();
            return connection;
        }

        public IConnection GetOrAddConnection(string name, string path, string rCommandLineArguments, bool isUserCreated) {
            var newConnection = CreateConnection(name, path, rCommandLineArguments, isUserCreated);
            var connection = _userConnections.GetOrAdd(newConnection.Name, newConnection);
            UpdateRecentConnections();
            return connection;
        }

        public bool TryRemove(string name) {
            IConnection connection;
            var isRemoved = _userConnections.TryRemove(name, out connection);
            if (isRemoved) {
                UpdateRecentConnections();

                // Credentials are saved by URI (connection id). Delete the credentials if there are no other connections using it.
                if (_userConnections.All(kvp => kvp.Value.Id != connection.Id)) {
                    _securityService.DeleteUserCredentials(connection.Id.ToCredentialAuthority());
                }
            }
            return isRemoved;
        }

        public Task TestConnectionAsync(IConnectionInfo connection, CancellationToken cancellationToken = default(CancellationToken)) {
            return _sessionProvider.TestBrokerConnectionAsync(connection.Name, connection.Path, cancellationToken);
        }

        public async Task ReconnectAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            var connection = ActiveConnection;
            if (connection != null && !_sessionProvider.IsConnected) {
                await _sessionProvider.TrySwitchBrokerAsync(connection.Name, connection.Path, cancellationToken);
            }
        }

        public async Task ConnectAsync(IConnectionInfo connection, CancellationToken cancellationToken = default(CancellationToken)) {
            if (ActiveConnection == null || !ActiveConnection.Path.PathEquals(connection.Path) || string.IsNullOrEmpty(_sessionProvider.Broker.Name)) {
                await TrySwitchBrokerAsync(connection, cancellationToken);
                await _shell.SwitchToMainThreadAsync();
                var interactiveWindow = await _interactiveWorkflow.GetOrCreateVisualComponentAsync();
                interactiveWindow.Container.Show(focus: false, immediate: false);
            }
        }
        
        public Task<bool> TryConnectToPreviouslyUsedAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            var connectionInfo = _settings.LastActiveConnection;
            if (connectionInfo != null) {
                var c = GetOrCreateConnection(connectionInfo.Name, connectionInfo.Path, connectionInfo.RCommandLineArguments, connectionInfo.IsUserCreated);
                if (c.IsRemote) {
                    return Task.FromResult(false); // Do not restore remote connections automatically
                }
            }

            return !string.IsNullOrEmpty(connectionInfo?.Path) 
                ? TrySwitchBrokerAsync(connectionInfo, cancellationToken) 
                : Task.FromResult(false);
        }

        private Task<bool> TrySwitchBrokerAsync(IConnectionInfo info, CancellationToken cancellationToken = default(CancellationToken)) {
            var connection = GetOrCreateConnection(info.Name, info.Path, info.RCommandLineArguments, info.IsUserCreated);
            return TrySwitchBrokerAsync(connection, cancellationToken);
        }

        private async Task<bool> TrySwitchBrokerAsync(IConnection connection, CancellationToken cancellationToken = default(CancellationToken)) {
            var brokerSwitched = await _sessionProvider.TrySwitchBrokerAsync(connection.Name, connection.Path, cancellationToken);
            if (brokerSwitched) {
                ActiveConnection = connection;
                SaveActiveConnectionToSettings();
            }
            return brokerSwitched;
        }

        private IConnection CreateConnection(string name, string path, string rCommandLineArguments, bool isUserCreated) =>
            new Connection(name, path, rCommandLineArguments, DateTime.Now, isUserCreated);

        private IConnection GetOrCreateConnection(string name, string path, string rCommandLineArguments, bool isUserCreated) {
            var newConnection = CreateConnection(name, path, rCommandLineArguments, isUserCreated);
            IConnection connection;
            return _userConnections.TryGetValue(newConnection.Name, out connection) ? connection : newConnection;
        }

        private IConnection UpdateConnectionFactory(IConnection oldConnection, IConnection newConnection) {
            if (oldConnection != null && newConnection.Equals(oldConnection)) {
                return oldConnection;
            }

            UpdateActiveConnection();
            return newConnection;
        }

        private Dictionary<string, IConnection> GetConnectionsFromSettings() => _settings.Connections
            .Select(c => CreateConnection(c.Name, c.Path, c.RCommandLineArguments, c.IsUserCreated))
            .ToDictionary(k => k.Name);

        private void SaveConnectionsToSettings() {
            _settings.Connections = RecentConnections
                .Select(c => new ConnectionInfo {
                    Name = c.Name,
                    Path = c.Path,
                    RCommandLineArguments = c.RCommandLineArguments,
                    IsUserCreated = c.IsUserCreated
                })
                .ToArray();
        }

        private void UpdateRecentConnections(bool save = true) {
            RecentConnections = new ReadOnlyCollection<IConnection>(_userConnections.Values.OrderByDescending(c => c.LastUsed).ToList());
            if (save) {
                SaveConnectionsToSettings();
            }
            RecentConnectionsChanged?.Invoke(this, new EventArgs());
        }

        private Dictionary<string, IConnection> CreateConnectionList() {
            var connections = GetConnectionsFromSettings();
            var localEngines = new RInstallation().GetCompatibleEngines();

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
                    connections[e.Name] = CreateConnection(e.Name, e.InstallPath, string.Empty, isUserCreated: false);
                }
            }

            // Verify that most recently used connection is still valid
            var last = _settings.LastActiveConnection;
            if (last != null && !IsRemoteConnection(last.Path) && !IsValidLocalConnection(last.Name, last.Path)) {
                _settings.LastActiveConnection = null;
            }

            if (connections.Count == 0) {
                if (!localEngines.Any()) {
                    var message = string.Format(CultureInfo.InvariantCulture, Resources.NoLocalR, Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    if (_shell.ShowMessage(message, MessageButtons.YesNo) == MessageButtons.Yes) {
                        var installer = _shell.ExportProvider.GetExportedValue<IMicrosoftRClientInstaller>();
                        installer.LaunchRClientSetup(_shell);
                        return connections;
                    }
                }
                // No connections, may be first use or connections were removed.
                // Add local connections so there is at least something available.
                foreach (var e in localEngines) {
                    var c = CreateConnection(e.Name, e.InstallPath, string.Empty, isUserCreated: false);
                    connections[e.Name] = c;
                }
            }
            return connections;
        }

        private bool IsValidLocalConnection(string name, string path) {
            try {
                var info = new RInterpreterInfo(name, path);
                return info.VerifyInstallation();
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                _shell.Services.Log.WriteAsync(LogVerbosity.Normal, MessageCategory.Error, ex.Message).DoNotWait();
            }
            return false;
        }

        private bool IsRemoteConnection(string path) {
            try {
                Uri uri;
                return Uri.TryCreate(path, UriKind.Absolute, out uri) && !uri.IsFile;
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                _shell.Services.Log.WriteAsync(LogVerbosity.Normal, MessageCategory.Error, ex.Message).DoNotWait();
            }
            return false;
        }

        private void BrokerStateChanged(object sender, BrokerStateChangedEventArgs eventArgs) {
            IsConnected = eventArgs.IsConnected && _interactiveWorkflow.ActiveWindow != null;
            UpdateActiveConnection();
            ConnectionStateChanged?.Invoke(this, new ConnectionEventArgs(IsConnected, ActiveConnection));
        }

        private void ActiveWindowChanged(object sender, ActiveWindowChangedEventArgs eventArgs) {
            IsConnected = _sessionProvider.IsConnected && eventArgs.Window != null;
            ConnectionStateChanged?.Invoke(this, new ConnectionEventArgs(IsConnected, ActiveConnection));
        }

        private void UpdateActiveConnection() {
            if (string.IsNullOrEmpty(_sessionProvider.Broker.Name) || ActiveConnection?.Id == _sessionProvider.Broker.Uri) {
                return;
            }

            ActiveConnection = RecentConnections.FirstOrDefault(c => c.Id == _sessionProvider.Broker.Uri);
            SaveActiveConnectionToSettings();
        }

        private void SaveActiveConnectionToSettings() {
            _shell.DispatchOnUIThread(() => _settings.LastActiveConnection = ActiveConnection == null
                ? null
                : new ConnectionInfo {
                    Name = ActiveConnection.Name,
                    Path = ActiveConnection.Path,
                    RCommandLineArguments = ActiveConnection.RCommandLineArguments
                });
        }
    }
}