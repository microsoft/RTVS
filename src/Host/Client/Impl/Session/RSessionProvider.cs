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
using Microsoft.Common.Core.Threading;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Host.Client.Session {
    public class RSessionProvider : IRSessionProvider {
        private readonly IRSessionProviderCallback _callback;
        private readonly ConcurrentDictionary<Guid, RSession> _sessions = new ConcurrentDictionary<Guid, RSession>();
        private readonly DisposeToken _disposeToken = DisposeToken.Create<RSessionProvider>();
        private readonly BinaryAsyncLock _brokerSwitchLock = new BinaryAsyncLock();
        private readonly BinaryAsyncLock _brokerDisconnectedLock = new BinaryAsyncLock();
        private readonly AsyncCountdownEvent _connectCde = new AsyncCountdownEvent(0);

        private readonly BrokerClientProxy _brokerProxy;
        private int _sessionCounter;
        private int _isConnected;

        public bool IsConnected => _isConnected == 1;

        public IBrokerClient Broker => _brokerProxy;

        public event EventHandler BrokerChanging;
        public event EventHandler BrokerChangeFailed;
        public event EventHandler BrokerChanged;
        public event EventHandler<BrokerStateChangedEventArgs> BrokerStateChanged;

        public RSessionProvider(IRSessionProviderCallback callback = null) {
            _callback = callback ?? new NullRSessionProviderCallback();
            _brokerProxy = new BrokerClientProxy(_connectCde);
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

            foreach (var session in _sessions.Values) {
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
            _brokerDisconnectedLock.ResetAsync().DoNotWait();
        }

        private void RSessionOnDisconnected(object sender, EventArgs e) {
            RSessionOnDisconnectedAsync().DoNotWait();
        }

        private async Task RSessionOnDisconnectedAsync() {
            var token = await _brokerDisconnectedLock.WaitAsync();
            try {
                // We don't want to show that connection is broken just because one of the sessions has been disconnected. Need to test connection
                await TestBrokerConnectionWithRHost(_brokerProxy, default(CancellationToken));
                token.Reset();
            } catch (RHostDisconnectedException) {
                token.Set();
                OnBrokerDisconnected();
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
                await TestBrokerConnectionWithRHost(brokerClient, cancellationToken);
            } finally {
                brokerClient.Dispose();
            }
        }

        private static async Task TestBrokerConnectionWithRHost(IBrokerClient brokerClient, CancellationToken cancellationToken) {
            var callbacks = new NullRCallbacks();
            var rhost = await brokerClient.ConnectAsync(nameof(TestBrokerConnectionAsync), callbacks, cancellationToken: cancellationToken);
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

            if (brokerClient.Name.EqualsOrdinal(_brokerProxy.Name) &&
                brokerClient.Uri.AbsoluteUri.PathEquals(_brokerProxy.Uri.AbsoluteUri)) {

                brokerClient.Dispose();
                // Switching to the broker that is currently running and connected is always successful
                if (IsConnected) {
                    return true;
                }

                return await TryReconnectAsync(cancellationToken);
            }

            // Connector switching shouldn't be concurrent
            IBinaryAsyncLockToken lockToken;
            try {
                lockToken = await _brokerSwitchLock.WaitAsync(cancellationToken);
                await _connectCde.WaitAsync(cancellationToken);
            } catch (OperationCanceledException) {
                brokerClient.Dispose();
                return false;
            }

            // First switch broker proxy so that all new sessions are created for the new broker
            var oldBroker = _brokerProxy.Set(brokerClient);
            try {
                BrokerChanging?.Invoke(this, EventArgs.Empty);
                await SwitchBrokerAsync(cancellationToken, oldBroker);
                oldBroker.Dispose();
                PrintBrokerInformation();
            } catch(Exception ex) {
                _brokerProxy.Set(oldBroker);
                brokerClient.Dispose();
                BrokerChangeFailed?.Invoke(this, EventArgs.Empty);
                if (ex is OperationCanceledException || ex is RHostBinaryMissingException) { // RHostDisconnectedException is derived from OperationCanceledException
                    return false;
                }
                throw;
            } finally {
                lockToken.Reset();
            }

            OnBrokerConnected();
            BrokerChanged?.Invoke(this, new EventArgs());
            return true;
        }

        private async Task SwitchBrokerAsync(CancellationToken cancellationToken, IBrokerClient oldBroker) {
            var sessions = _sessions.Values.ToList();
            if (sessions.Any()) {
                await SwitchSessionsAsync(sessions, oldBroker, cancellationToken);
            } else {
                // Ping isn't enough here - need a "full" test with RHost
                await TestBrokerConnectionWithRHost(_brokerProxy, cancellationToken);
            }
        }

        private async Task SwitchSessionsAsync(IEnumerable<RSession> sessions, IBrokerClient oldBroker, CancellationToken cancellationToken) {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // All sessions should participate in switch. If any of it didn't start, cancel the rest.
            var startTransactionTasks = sessions.Select(s => s.StartSwitchingBrokerAsync(cts.Token)).ToList();

            try {
                await Task.WhenAll(startTransactionTasks);
                var transactions = startTransactionTasks.Select(t => t.Result).ToList();

                await Task.WhenAll(transactions.Select(t => ConnectToNewBrokerAsync(t, cts)));

                OnBrokerDisconnected();
                await Task.WhenAll(transactions.Select(t => CompleteSwitchingBrokerAsync(t, oldBroker, cts)));
            } finally {
                foreach (var task in startTransactionTasks.Where(t => t.Status == TaskStatus.RanToCompletion)) {
                    task.Result.Dispose();
                }
            }
        }

        private async Task<bool> TryReconnectAsync(CancellationToken cancellationToken) {
            // Connector switching shouldn't be concurrent
            IBinaryAsyncLockToken lockToken;
            try {
                lockToken = await _brokerSwitchLock.WaitAsync(cancellationToken);
                await _connectCde.WaitAsync(cancellationToken);
            } catch (OperationCanceledException) {
                return false;
            }

            try {
                await ReconnectAsync(cancellationToken);
            } catch (Exception) {
                return false;
            } finally {
                lockToken.Reset();
            }

            OnBrokerConnected();
            return true;
        }

        private async Task ReconnectAsync(CancellationToken cancellationToken) {
            var sessions = _sessions.Values.ToList();
            if (sessions.Any()) {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                // All sessions should participate in reconnect. If any of it didn't start, cancel the rest.
                var startTransactionTasks = sessions.Select(s => s.StartReconnectingAsync(cts.Token)).ToList();

                try {
                    await Task.WhenAll(startTransactionTasks);
                    var transactions = startTransactionTasks.Select(t => t.Result).ToList();

                    await Task.WhenAll(transactions.Select(s => ReconnectSessionAsync(s, cts)));
                } finally {
                    foreach (var task in startTransactionTasks.Where(t => t.Status == TaskStatus.RanToCompletion)) {
                        task.Result.Dispose();
                    }
                }
            } else {
                await TestBrokerConnectionWithRHost(_brokerProxy, cancellationToken);
            }
        }

        private async Task ReconnectSessionAsync(IRSessionReconnectTransaction transaction, CancellationTokenSource cts) {
            try {
                await transaction.ReconnectAsync();
            } catch (OperationCanceledException ex) when (!(ex is RHostDisconnectedException)) {
                // Swallow cancellation if it is a result of another session failure
                if (!cts.IsCancellationRequested) {
                    _callback.WriteConsole(Resources.RSessionProvider_SwitchingWorkspaceCanceled.FormatInvariant(_brokerProxy.Name, GetUriString(_brokerProxy)));
                    cts.Cancel();
                    throw;
                }
            } catch (Exception ex) {
                _callback.WriteConsole(Resources.RSessionProvider_SwitchingWorkspaceFailed.FormatInvariant(_brokerProxy.Name, GetUriString(_brokerProxy), ex.Message));
                cts.Cancel();
                throw;
            }
        }

        public void PrintBrokerInformation() {
            var a = _brokerProxy.AboutHost;

            _callback.WriteConsole(Resources.RServices_Information);
            _callback.WriteConsole("\t" + Resources.Version.FormatInvariant(a.Version));
            _callback.WriteConsole("\t" + Resources.OperatingSystem.FormatInvariant(a.OS.VersionString));
            _callback.WriteConsole("\t" + Resources.PlatformBits.FormatInvariant(a.Is64BitOperatingSystem ? Resources.Bits64 : Resources.Bits32));
            _callback.WriteConsole("\t" + Resources.ProcessBits.FormatInvariant(a.Is64BitProcess ? Resources.Bits64 : Resources.Bits32));
            _callback.WriteConsole("\t" + Resources.ProcessorCount.FormatInvariant(a.ProcessorCount));
            _callback.WriteConsole("\t" + Resources.TotalPhysicalMemory.FormatInvariant(a.TotalPhysicalMemory));
            _callback.WriteConsole("\t" + Resources.FreePhysicalMemory.FormatInvariant(a.FreePhysicalMemory));
            _callback.WriteConsole("\t" + Resources.TotalVirtualMemory.FormatInvariant(a.TotalVirtualMemory));
            _callback.WriteConsole("\t" + Resources.FreeVirtualMemory.FormatInvariant(a.FreeVirtualMemory));

            _callback.WriteConsole(Resources.InstalledInterpreters);
            foreach (var name in a.Interpreters) {
                _callback.WriteConsole("\t" + name);
            }
        }

        private async Task ConnectToNewBrokerAsync(IRSessionSwitchBrokerTransaction transaction, CancellationTokenSource cts) {
            try {
                await transaction.ConnectToNewBrokerAsync();
            } catch (OperationCanceledException ex) when (!(ex is RHostDisconnectedException)) {
                // Swallow cancellation if it is a result of another session failure
                if (!cts.IsCancellationRequested) {
                    _callback.WriteConsole(Resources.RSessionProvider_SwitchingWorkspaceCanceled.FormatInvariant(_brokerProxy.Name, GetUriString(_brokerProxy)));
                    cts.Cancel();
                    throw;
                }
            } catch (Exception ex) {
                _callback.WriteConsole(Resources.RSessionProvider_SwitchingWorkspaceFailed.FormatInvariant(_brokerProxy.Name, GetUriString(_brokerProxy), ex.Message));
                cts.Cancel();
                throw;
            }
        }

        private async Task CompleteSwitchingBrokerAsync(IRSessionSwitchBrokerTransaction transaction, IBrokerClient oldBroker, CancellationTokenSource cts) {
            try {
                await transaction.CompleteSwitchingBrokerAsync();
            } catch (OperationCanceledException ex) when (!(ex is RHostDisconnectedException)) {
                // Swallow cancellation if it is a result of another session failure
                if (!cts.IsCancellationRequested) {
                    _callback.WriteConsole(Resources.RSessionProvider_RestartingSessionAfterSwitchingCanceled.FormatInvariant(_brokerProxy.Name, GetUriString(_brokerProxy)));
                    cts.Cancel();
                }
            } catch (Exception ex) {
                var switchingFromNull = oldBroker is NullBrokerClient;
                var message = switchingFromNull
                    ? Resources.RSessionProvider_StartingSessionAfterSwitchingFailed
                    : Resources.RSessionProvider_RestartingSessionAfterSwitchingFailed.FormatInvariant(_brokerProxy.Name, GetUriString(_brokerProxy), ex.Message, oldBroker.Name, GetUriString(oldBroker));

                _callback.WriteConsole(message);
                cts.Cancel();
                throw;
            }
        }

        private async Task<IBrokerClient> CreateBrokerClientAsync(string name, string path) {
            path = path ?? new RInstallation().GetCompatibleEngines().FirstOrDefault()?.InstallPath;

            Uri uri;
            if (!Uri.TryCreate(path, UriKind.Absolute, out uri)) {
                return null;
            }

            if (uri.IsFile) {
                return new LocalBrokerClient(name, uri.LocalPath) as IBrokerClient;
            }

            var windowHandle = await _callback.GetApplicationWindowHandleAsync();
            return new RemoteBrokerClient(name, uri, windowHandle);
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

        private static string GetUriString(IBrokerClient connector)
            => connector.Uri.IsFile ? connector.Uri.LocalPath : connector.Uri.AbsoluteUri;
    }
}