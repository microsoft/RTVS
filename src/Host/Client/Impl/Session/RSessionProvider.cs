// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Protocol;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Host.Client.Session {
    public class RSessionProvider : IRSessionProvider {
        private readonly ConcurrentDictionary<Guid, RSession> _sessions = new ConcurrentDictionary<Guid, RSession>();
        private readonly DisposeToken _disposeToken = DisposeToken.Create<RSessionProvider>();
        private readonly AsyncReaderWriterLock _connectArwl = new AsyncReaderWriterLock();

        private readonly BrokerClientProxy _brokerProxy;
        private readonly ICoreServices _services;
        private readonly IConsole _console;

        private int _sessionCounter;
        private Task _updateHostLoadLoopTask;
        private HostLoad _hostLoad;

        public bool IsConnected => _hostLoad != null;

        public IBrokerClient Broker => _brokerProxy;

        public event EventHandler BrokerChanging;
        public event EventHandler BrokerChangeFailed;
        public event EventHandler BrokerChanged;
        public event EventHandler<BrokerStateChangedEventArgs> BrokerStateChanged;

        public RSessionProvider(ICoreServices services, IConsole callback = null) {
            _console = callback ?? new NullConsole();
            _brokerProxy = new BrokerClientProxy();
            _services = services;
        }

        public IRSession GetOrCreate(Guid guid) {
            _disposeToken.ThrowIfDisposed();
            return _sessions.GetOrAdd(guid, id => CreateRSession(guid));
        }

        public IEnumerable<IRSession> GetSessions() {
            return _sessions.Values;
        }

        public void Dispose() {
            if (!_disposeToken.TryMarkDisposed()) {
                return;
            }

            var sessions = GetSessions().ToList();
            var stopHostTasks = sessions.Select(session => session.StopHostAsync());
            try {
                _services.Tasks.Wait(Task.WhenAll(stopHostTasks));
            } catch (Exception ex) when (!ex.IsCriticalException()) { }

            foreach (var session in sessions) {
                session.Dispose();
            }

            Broker.Dispose();
        }

        private RSession CreateRSession(Guid guid) {
            var session = new RSession(Interlocked.Increment(ref _sessionCounter), Broker, _connectArwl.CreateExclusiveReaderLock(), () => DisposeSession(guid));
            session.Connected += RSessionOnConnected;
            return session;
        }

        private void DisposeSession(Guid guid) {
            RSession session;
            if (_sessions.TryRemove(guid, out session)) {
                session.Connected -= RSessionOnConnected;
            }
        }

        private void RSessionOnConnected(object sender, RConnectedEventArgs e) {
            if (_hostLoad == null) {
                UpdateHostLoadAsync().DoNotWait();
            }
        }

        private void OnBrokerStateChanged(HostLoad hostLoad) {
            Interlocked.Exchange(ref _hostLoad, hostLoad);
            var args = new BrokerStateChangedEventArgs(hostLoad != null, hostLoad ?? new HostLoad());
            Task.Run(() => BrokerStateChanged?.Invoke(this, args)).DoNotWait();
        }
        
        private void OnBrokerChanged() {
            Task.Run(() => BrokerChanged?.Invoke(this, new EventArgs())).DoNotWait();
        }
        
        public async Task TestBrokerConnectionAsync(string name, string path, CancellationToken cancellationToken = default(CancellationToken)) {
            using (_disposeToken.Link(ref cancellationToken)) {
                await TaskUtilities.SwitchToBackgroundThread();

                // Create random name to avoid collision with actual broker client
                name = name + Guid.NewGuid().ToString("N");
                var brokerClient = CreateBrokerClient(name, path, cancellationToken);
                if (brokerClient == null) {
                    throw new ArgumentException(nameof(path));
                }

                using (brokerClient) {
                    await TestBrokerConnectionWithRHost(brokerClient, cancellationToken);
                }
            }
        }

        private static async Task TestBrokerConnectionWithRHost(IBrokerClient brokerClient, CancellationToken cancellationToken) {
            var callbacks = new NullRCallbacks();
            var connectionInfo = new BrokerConnectionInfo(nameof(TestBrokerConnectionAsync), callbacks);
            var rhost = await brokerClient.ConnectAsync(connectionInfo, cancellationToken);
            try {
                var rhostRunTask = rhost.Run(cancellationToken);
                callbacks.SetReadConsoleInput("q()\n");
                await rhostRunTask;
            } finally {
                rhost.Dispose();
            }
        }

        public async Task<bool> TrySwitchBrokerAsync(string name, string path = null, CancellationToken cancellationToken = default(CancellationToken)) {
            using (_disposeToken.Link(ref cancellationToken)) {
                await TaskUtilities.SwitchToBackgroundThread();

                var brokerClient = CreateBrokerClient(name, path, cancellationToken);
                if (brokerClient == null) {
                    return false;
                }

                // Broker switching shouldn't be concurrent
                IAsyncReaderWriterLockToken lockToken;
                try {
                    lockToken = await _connectArwl.WriterLockAsync(cancellationToken);
                } catch (OperationCanceledException) {
                    brokerClient.Dispose();
                    return false;
                }

                if (brokerClient.Name.EqualsOrdinal(_brokerProxy.Name) &&
                    brokerClient.Uri.AbsoluteUri.PathEquals(_brokerProxy.Uri.AbsoluteUri)) {

                    brokerClient.Dispose();

                    try {
                        // Switching to the broker that is currently running and connected is always successful
                        if (IsConnected) {
                            return true;
                        }

                        await ReconnectAsync(cancellationToken);
                    } catch (Exception) {
                        return false;
                    } finally {
                        lockToken.Dispose();
                    }

                    OnBrokerStateChanged(new HostLoad());
                    return true;
                }

                // First switch broker proxy so that all new sessions are created for the new broker
                var oldBroker = _brokerProxy.Set(brokerClient);
                if (_updateHostLoadLoopTask == null) {
                    _updateHostLoadLoopTask = UpdateHostLoadLoopAsync();
                }

                try {
                    BrokerChanging?.Invoke(this, EventArgs.Empty);
                    await SwitchBrokerAsync(cancellationToken);
                    oldBroker.Dispose();
                } catch (Exception ex) {
                    _brokerProxy.Set(oldBroker);
                    if (_disposeToken.IsDisposed) {
                        oldBroker.Dispose();
                    }
                    brokerClient.Dispose();
                    BrokerChangeFailed?.Invoke(this, EventArgs.Empty);
                    if (ex is OperationCanceledException || ex is RHostBrokerBinaryMissingException) {
                        // RHostDisconnectedException is derived from OperationCanceledException
                        return false;
                    }
                    throw;
                } finally {
                    lockToken.Dispose();
                }

                OnBrokerStateChanged(new HostLoad());
                OnBrokerChanged();
                return true;
            }
        }

        private async Task SwitchBrokerAsync(CancellationToken cancellationToken) {
            var transactions = new List<IRSessionSwitchBrokerTransaction>();
            var sessionsToStop = new List<IRSession>();

            foreach (var session in _sessions.Values) {
                var transaction = session.StartSwitchingBroker();
                if (transaction != null) {
                    transactions.Add(transaction);
                } else {
                    sessionsToStop.Add(session);
                }
            }

            if (transactions.Any()) {
                await SwitchSessionsAsync(transactions, sessionsToStop, cancellationToken);
            } else {
                // Ping isn't enough here - need a "full" test with RHost
                await TestBrokerConnectionWithRHost(_brokerProxy, cancellationToken);
                await StopSessionsAsync(sessionsToStop);
            }
        }

        private async Task SwitchSessionsAsync(IReadOnlyCollection<IRSessionSwitchBrokerTransaction> transactions, List<IRSession> sessionsToStop, CancellationToken cancellationToken) {
            // All sessions should participate in switch. If any of it didn't start, cancel the rest.
            try {
                await ConnectToNewBrokerAsync(transactions, cancellationToken);
                await Task.WhenAll(CompleteSwitchingBrokerAsync(transactions, cancellationToken), StopSessionsAsync(sessionsToStop));
            } finally {
                foreach (var transaction in transactions) {
                    transaction.Dispose();
                }
            }
        }
        
        private Task StopSessionsAsync(IEnumerable<IRSession> sessions) {
            var stopSessionsTask = Task.WhenAll(sessions.Select(s => s.StopHostAsync()));
            OnBrokerStateChanged(null);
            return stopSessionsTask;
        }

        private async Task ReconnectAsync(CancellationToken cancellationToken) {
            var sessions = _sessions.Values.ToList();
            if (sessions.Any()) {
                try {
                    await WhenAllCancelOnFailure(sessions, (s, ct) => s.ReconnectAsync(ct), cancellationToken);
                } catch (OperationCanceledException ex) when (!(ex is RHostDisconnectedException)) {
                    throw;
                } catch (Exception ex) {
                    _console.Write(Resources.RSessionProvider_ConnectionFailed.FormatInvariant(ex.Message));
                    throw;
                }
            } else {
                await TestBrokerConnectionWithRHost(_brokerProxy, cancellationToken);
            }
        }

        private async Task ConnectToNewBrokerAsync(IEnumerable<IRSessionSwitchBrokerTransaction> transactions, CancellationToken cancellationToken) {
            try {
                await WhenAllCancelOnFailure(transactions, ConnectToNewBrokerAsync, cancellationToken);
            } catch (OperationCanceledException ex) when (!(ex is RHostDisconnectedException)) {
                throw;
            } catch (Exception ex) {
                _console.Write(Resources.RSessionProvider_ConnectionFailed.FormatInvariant(ex.Message));
                throw;
            }
        }

        private static Task ConnectToNewBrokerAsync(IRSessionSwitchBrokerTransaction transaction, CancellationToken cancellationToken)
            => transaction.ConnectToNewBrokerAsync(cancellationToken);

        private async Task CompleteSwitchingBrokerAsync(IEnumerable<IRSessionSwitchBrokerTransaction> transactions, CancellationToken cancellationToken) {
            try {
                await WhenAllCancelOnFailure(transactions, CompleteSwitchingBrokerAsync, cancellationToken);
            } catch (OperationCanceledException ex) when (!(ex is RHostDisconnectedException)) {
            } catch (Exception ex) {
                _console.Write(Resources.RSessionProvider_ConnectionFailed.FormatInvariant(ex.Message));
                throw;
            }
        }

        private static Task CompleteSwitchingBrokerAsync(IRSessionSwitchBrokerTransaction transaction, CancellationToken cancellationToken) 
            => transaction.CompleteSwitchingBrokerAsync(cancellationToken);

        private static Task WhenAllCancelOnFailure(IEnumerable<IRSessionSwitchBrokerTransaction> transactions, Func<IRSessionSwitchBrokerTransaction, CancellationToken, Task> taskFactory, CancellationToken cancellationToken) {
            return TaskUtilities.WhenAllCancelOnFailure(transactions, async (t, ct) => {
                try {
                    await taskFactory(t, ct);
                } catch (ObjectDisposedException) when (t.IsSessionDisposed) {
                    ct.ThrowIfCancellationRequested();
                } catch (OperationCanceledException) when (t.IsSessionDisposed) {
                    ct.ThrowIfCancellationRequested();
                }
            }, cancellationToken);
        }

        private static Task WhenAllCancelOnFailure(IEnumerable<RSession> sessions, Func<RSession, CancellationToken, Task> taskFactory, CancellationToken cancellationToken) {
            return TaskUtilities.WhenAllCancelOnFailure(sessions, async (s, ct) => {
                try {
                    await taskFactory(s, ct);
                } catch (ObjectDisposedException) when (s.IsDisposed) {
                    ct.ThrowIfCancellationRequested();
                } catch (OperationCanceledException) when (s.IsDisposed) {
                    ct.ThrowIfCancellationRequested();
                }
            }, cancellationToken);
        }

        private IBrokerClient CreateBrokerClient(string name, string path, CancellationToken cancellationToken) {
            path = path ?? new RInstallation().GetCompatibleEngines().FirstOrDefault()?.InstallPath;

            Uri uri;
            if (!Uri.TryCreate(path, UriKind.Absolute, out uri)) {
                return null;
            }

            if (uri.IsFile) {
                return new LocalBrokerClient(name, uri.LocalPath, _services, _console);
            }

            return new RemoteBrokerClient(name, uri, _services, _console, cancellationToken);
        }

        private async Task UpdateHostLoadLoopAsync() {
            while (!_disposeToken.IsDisposed) {
                var ct = _disposeToken.CancellationToken;

                await Task.Delay(2000, ct);
                await UpdateHostLoadAsync(ct);
            }
        }

        private async Task UpdateHostLoadAsync(CancellationToken ct = default(CancellationToken)) {
            using (await _connectArwl.ReaderLockAsync(ct)) {
                try {
                    var hostLoad = await Broker.GetHostInformationAsync<HostLoad>(ct);
                    OnBrokerStateChanged(hostLoad);
                } catch (RHostDisconnectedException) {
                }
            }
        }
    }
}