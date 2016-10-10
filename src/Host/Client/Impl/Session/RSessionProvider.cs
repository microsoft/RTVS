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
using Microsoft.Common.Core.Telemetry;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Host.Client.Session {
    public class RSessionProvider : IRSessionProvider {
        private readonly IRSessionProviderCallback _callback;
        private readonly ConcurrentDictionary<Guid, RSession> _sessions = new ConcurrentDictionary<Guid, RSession>();
        private readonly DisposeToken _disposeToken = DisposeToken.Create<RSessionProvider>();
        private readonly BinaryAsyncLock _brokerDisconnectedLock = new BinaryAsyncLock();
        private readonly AsyncReaderWriterLock _connectArwl = new AsyncReaderWriterLock();

        private readonly BrokerClientProxy _brokerProxy;
        private readonly ICoreServices _services;

        private int _sessionCounter;
        private int _isConnected;

        public bool IsConnected => _isConnected == 1;

        public IBrokerClient Broker => _brokerProxy;

        public event EventHandler BrokerChanging;
        public event EventHandler BrokerChangeFailed;
        public event EventHandler BrokerChanged;
        public event EventHandler<BrokerStateChangedEventArgs> BrokerStateChanged;

        public RSessionProvider(ICoreServices services, IRSessionProviderCallback callback = null) {
            _callback = callback ?? new NullRSessionProviderCallback();
            _brokerProxy = new BrokerClientProxy(_connectArwl);
            _services = services;
        }

        public IRSession GetOrCreate(Guid guid) {
            _disposeToken.ThrowIfDisposed();
            return _sessions.GetOrAdd(guid, id => CreateRSession(guid));
        }

        public IEnumerable<IRSession> GetSessions() {
            return _sessions.Values;
        }

        public async Task<IRSessionEvaluation> BeginEvaluationAsync(RHostStartupInfo startupInfo,
            CancellationToken cancellationToken = default(CancellationToken)) {
            var session = GetOrCreate(Guid.NewGuid());
            cancellationToken.ThrowIfCancellationRequested();

            try {
                await session.StartHostAsync(new RHostStartupInfo {
                    Name = "IsolatedRHost" + session.Id,
                    CranMirrorName = startupInfo.CranMirrorName,
                    CodePage = startupInfo.CodePage
                }, null);
                cancellationToken.ThrowIfCancellationRequested();

                var evaluation = await session.BeginEvaluationAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                return new IsolatedRSessionEvaluation(session, evaluation);
            } finally {
                if (cancellationToken.IsCancellationRequested) {
                    await session.StopHostAsync();
                }
            }
        }

        public void Dispose() {
            if (!_disposeToken.TryMarkDisposed()) {
                return;
            }

            var sessions = GetSessions();
            var stopHostTasks = sessions.Select(session => session.StopHostAsync());
            try {
                Task.WhenAll(stopHostTasks).GetAwaiter().GetResult();
            } catch (Exception ex) when (!ex.IsCriticalException()) {
            }

            foreach (var session in sessions) {
                session.Dispose();
            }

            Broker.Dispose();
        }

        private RSession CreateRSession(Guid guid) {
            var session = new RSession(Interlocked.Increment(ref _sessionCounter), Broker, () => DisposeSession(guid));
            session.Connected += RSessionOnConnected;
            session.Disconnected += RSessionOnDisconnected;
            return session;
        }

        private void DisposeSession(Guid guid) {
            RSession session;
            if (_sessions.TryRemove(guid, out session)) {
                session.Connected -= RSessionOnConnected;
                session.Disconnected -= RSessionOnDisconnected;
            }
        }

        private void RSessionOnConnected(object sender, RConnectedEventArgs e) {
            OnBrokerConnected();
            _brokerDisconnectedLock.EnqueueReset();
        }

        private void RSessionOnDisconnected(object sender, EventArgs e) {
            RSessionOnDisconnectedAsync().DoNotWait();
        }

        private async Task RSessionOnDisconnectedAsync() {
            var token = await _brokerDisconnectedLock.WaitAsync();
            try {
                if (!token.IsSet) {
                    // We don't want to show that connection is broken just because one of the sessions has been disconnected. Need to test connection
                    await TestBrokerConnectionWithRHost(_brokerProxy, default(CancellationToken), default(ReentrancyToken));
                    token.Reset();
                }
            } catch (RHostDisconnectedException) {
                token.Set();
                OnBrokerDisconnected();
            } catch (Exception) {
                token.Reset();
            }
        }

        private void OnBrokerConnected() {
            if (Interlocked.Exchange(ref _isConnected, 1) == 0) {
                BrokerStateChanged?.Invoke(this, new BrokerStateChangedEventArgs(true));
            }
        }

        private void OnBrokerDisconnected() {
            if (Interlocked.Exchange(ref _isConnected, 0) == 1) {
                BrokerStateChanged?.Invoke(this, new BrokerStateChangedEventArgs(false));
            }
        }

        public async Task TestBrokerConnectionAsync(string name, string path, CancellationToken cancellationToken = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();

            // Create random name to avoid collision with actual broker client
            name = name + Guid.NewGuid().ToString("N");
            var brokerClient = await CreateBrokerClientAsync(name, path);
            if (brokerClient == null) {
                throw new ArgumentException(nameof(path));
            }

            try {
                await TestBrokerConnectionWithRHost(brokerClient, cancellationToken, default(ReentrancyToken));
            } finally {
                brokerClient.Dispose();
            }
        }

        private static async Task TestBrokerConnectionWithRHost(IBrokerClient brokerClient, CancellationToken cancellationToken, ReentrancyToken reentrancyToken) {

            var callbacks = new NullRCallbacks();
            var rhost = await brokerClient.ConnectAsync(nameof(TestBrokerConnectionAsync), callbacks, cancellationToken: cancellationToken, reentrancyToken: reentrancyToken);
            try {
                var rhostRunTask = rhost.Run(cancellationToken);
                callbacks.SetReadConsoleInput("q()\n");
                await rhostRunTask;
            } finally {
                rhost.Dispose();
            }
        }

        public async Task<bool> TrySwitchBrokerAsync(string name, string path = null, CancellationToken cancellationToken = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();

            var brokerClient = await CreateBrokerClientAsync(name, path);
            if (brokerClient == null) {
                return false;
            }

            // Broker switching shouldn't be concurrent
            IAsyncReaderWriterLockToken lockToken = null;
            try {
                lockToken = await _connectArwl.WriterLockAsync(cancellationToken);
            } catch (OperationCanceledException) {
                lockToken?.Dispose();
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

                    await ReconnectAsync(cancellationToken, lockToken.Reentrancy);
                } catch (Exception) {
                    return false;
                } finally {
                    lockToken.Dispose();
                }

                OnBrokerConnected();
                return true;
            }

            // First switch broker proxy so that all new sessions are created for the new broker
            var oldBroker = _brokerProxy.Set(brokerClient);
            try {
                BrokerChanging?.Invoke(this, EventArgs.Empty);
                await SwitchBrokerAsync(oldBroker, cancellationToken, lockToken.Reentrancy);
                oldBroker.Dispose();

                if (brokerClient.IsRemote) {
                    _callback.WriteConsole(Environment.NewLine + Resources.Connected + Environment.NewLine);
                    PrintBrokerInformation();
                }
            } catch (Exception ex) {
                _brokerProxy.Set(oldBroker);
                brokerClient.Dispose();
                BrokerChangeFailed?.Invoke(this, EventArgs.Empty);
                if (ex is OperationCanceledException || ex is RHostBrokerBinaryMissingException) { // RHostDisconnectedException is derived from OperationCanceledException
                    return false;
                }
                throw;
            } finally {
                lockToken.Dispose();
            }

            OnBrokerConnected();
            BrokerChanged?.Invoke(this, new EventArgs());
            return true;
        }

        private async Task SwitchBrokerAsync(IBrokerClient oldBroker, CancellationToken cancellationToken, ReentrancyToken reentrancyToken) {
            var sessions = _sessions.Values.ToList();
            if (sessions.Any()) {
                await SwitchSessionsAsync(sessions, cancellationToken, reentrancyToken);
            } else {
                // Ping isn't enough here - need a "full" test with RHost
                await TestBrokerConnectionWithRHost(_brokerProxy, cancellationToken, reentrancyToken);
            }
        }

        private async Task SwitchSessionsAsync(IEnumerable<RSession> sessions, CancellationToken cancellationToken, ReentrancyToken reentrancyToken) {
            // All sessions should participate in switch. If any of it didn't start, cancel the rest.
            var transactions = sessions.Select(s => s.StartSwitchingBroker()).ToList();

            try {
                await TaskUtilities.WhenAllCancelOnFailure(transactions, (t, ct) => t.AcquireLockAsync(ct), cancellationToken);
                await ConnectToNewBrokerAsync(transactions, cancellationToken, reentrancyToken);

                OnBrokerDisconnected();
                await CompleteSwitchingBrokerAsync(transactions, cancellationToken);
            } finally {
                foreach (var transaction in transactions) {
                    transaction.Dispose();
                }
            }
        }

        private async Task ReconnectAsync(CancellationToken cancellationToken, ReentrancyToken reentrancyToken) {
            var sessions = _sessions.Values.ToList();
            if (sessions.Any()) {
                // All sessions should participate in reconnect. If any of it didn't start, cancel the rest.
                var transactions = sessions.Select(s => s.StartReconnecting()).ToList();

                try {
                    await TaskUtilities.WhenAllCancelOnFailure(transactions, (t, ct) => t.AcquireLockAsync(ct), cancellationToken);
                    await TaskUtilities.WhenAllCancelOnFailure(transactions, (t, ct)=> t.ReconnectAsync(ct, reentrancyToken), cancellationToken);
                } catch (OperationCanceledException ex) when (!(ex is RHostDisconnectedException)) {
                    throw;
                } catch (Exception ex) {
                    _callback.WriteConsole(Resources.RSessionProvider_ConnectionFailed.FormatInvariant(ex.Message));
                    throw;
                } finally {
                    foreach (var transaction in transactions) {
                        transaction.Dispose();
                    }
                }
            } else {
                await TestBrokerConnectionWithRHost(_brokerProxy, cancellationToken, reentrancyToken);
            }
        }

        public void PrintBrokerInformation() {
            var a = _brokerProxy.AboutHost;

            _callback.WriteConsole(Environment.NewLine + Resources.RServices_Information);
            _callback.WriteConsole("\t" + Resources.Version.FormatInvariant(a.Version));
            _callback.WriteConsole("\t" + Resources.OperatingSystem.FormatInvariant(a.OS.VersionString));
            _callback.WriteConsole("\t" + Resources.PlatformBits.FormatInvariant(a.Is64BitOperatingSystem ? Resources.Bits64 : Resources.Bits32));
            _callback.WriteConsole("\t" + Resources.ProcessBits.FormatInvariant(a.Is64BitProcess ? Resources.Bits64 : Resources.Bits32));
            _callback.WriteConsole("\t" + Resources.ProcessorCount.FormatInvariant(a.ProcessorCount));
            _callback.WriteConsole("\t" + Resources.TotalPhysicalMemory.FormatInvariant(a.TotalPhysicalMemory));
            _callback.WriteConsole("\t" + Resources.FreePhysicalMemory.FormatInvariant(a.FreePhysicalMemory));
            _callback.WriteConsole("\t" + Resources.TotalVirtualMemory.FormatInvariant(a.TotalVirtualMemory));
            _callback.WriteConsole("\t" + Resources.FreeVirtualMemory.FormatInvariant(a.FreeVirtualMemory));

            // TODO: activate when we support switching between remote R interpreters in UI.
            //_callback.WriteConsole(Resources.InstalledInterpreters);
            foreach (var name in a.Interpreters) {
                _services.Telemetry.ReportEvent(TelemetryArea.Configuration, "Remote Interpteter", name);
                _services.Telemetry.ReportEvent(TelemetryArea.Configuration, "Remote OS", a.OS.VersionString);
                _services.Telemetry.ReportEvent(TelemetryArea.Configuration, "Remote CPUs", a.ProcessorCount);
                _services.Telemetry.ReportEvent(TelemetryArea.Configuration, "Remote RAM", a.TotalPhysicalMemory);
                //_callback.WriteConsole("\t" + name);
            }
        }

        private async Task ConnectToNewBrokerAsync(IEnumerable<IRSessionSwitchBrokerTransaction> transactions, CancellationToken cancellationToken, ReentrancyToken reentrancyToken) {
            try {
                await TaskUtilities.WhenAllCancelOnFailure(transactions, (t, ct) => t.ConnectToNewBrokerAsync(ct, reentrancyToken), cancellationToken);
            } catch (OperationCanceledException ex) when (!(ex is RHostDisconnectedException)) {
                throw;
            } catch (Exception ex) {
                _callback.WriteConsole(Resources.RSessionProvider_ConnectionFailed.FormatInvariant(ex.Message));
                throw;
            }
        }

        private async Task CompleteSwitchingBrokerAsync(IEnumerable<IRSessionSwitchBrokerTransaction> transactions, CancellationToken cancellationToken) {
            try {
                await TaskUtilities.WhenAllCancelOnFailure(transactions, (t, ct) => t.CompleteSwitchingBrokerAsync(ct), cancellationToken);
            } catch (OperationCanceledException ex) when (!(ex is RHostDisconnectedException)) {
            } catch (Exception ex) {
                _callback.WriteConsole(Resources.RSessionProvider_ConnectionFailed.FormatInvariant(ex.Message));
                throw;
            }
        }

        private async Task<IBrokerClient> CreateBrokerClientAsync(string name, string path) {
            path = path ?? new RInstallation().GetCompatibleEngines().FirstOrDefault()?.InstallPath;

            Uri uri;
            if (!Uri.TryCreate(path, UriKind.Absolute, out uri)) {
                return null;
            }

            var windowHandle = await _callback.GetApplicationWindowHandleAsync();
            if (uri.IsFile) {
                return new LocalBrokerClient(name, uri.LocalPath, _services, windowHandle) as IBrokerClient;
            }
            return new RemoteBrokerClient(name, uri, _services.Log, windowHandle);
        }

        private class IsolatedRSessionEvaluation : IRSessionEvaluation {
            private readonly IRSession _session;
            private readonly IRSessionEvaluation _evaluation;

            public IsolatedRSessionEvaluation(IRSession session, IRSessionEvaluation evaluation) {
                _session = session;
                _evaluation = evaluation;
            }

            public IReadOnlyList<IRContext> Contexts => _evaluation.Contexts;
            public bool IsMutating => _evaluation.IsMutating;

            public Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind, CancellationToken cancellationToken = new CancellationToken()) {
                return _evaluation.EvaluateAsync(expression, kind, cancellationToken);
            }

            public void Dispose() {
                _evaluation.Dispose();
                _session.StopHostAsync().ContinueWith(t => _session.Dispose());
            }
        }
    }
}