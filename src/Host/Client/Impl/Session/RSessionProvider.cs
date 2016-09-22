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
        }

        private void RSessionOnDisconnected(object sender, EventArgs e) {
            RSessionOnDisconnectedAsync().DoNotWait();
        }

        private async Task RSessionOnDisconnectedAsync() {
            try {
                // We don't want to show that connection is broken just because one of the sessions has been disconnected. Ping broker.
                await _brokerProxy.PingAsync();
            } catch (RHostDisconnectedException) {
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
                var callbacks = new NullRCallbacks();
                var rhost = await brokerClient.ConnectAsync(nameof(TestBrokerConnectionAsync), callbacks, cancellationToken: cancellationToken);
                try {
                    var rhostRunTask = rhost.Run(cancellationToken);
                    callbacks.SetReadConsoleInput("q()\n");
                    await rhostRunTask;
                } finally {
                    rhost.Dispose();
                }
            } finally {
                brokerClient.Dispose();
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
                // Switching to the broker that is currently running is always successful
                return true;
            }

            // Connector switching shouldn't be concurrent
            IBinaryAsyncLockToken lockToken;
            try {
                lockToken = await _brokerSwitchLock.WaitAsync(cancellationToken);
                await _connectCde.WaitAsync(cancellationToken);
            } catch (OperationCanceledException) {
                return false;
            }

            try {
                // First switch connector so that all new sessions are created for the new broker
                var oldBroker = _brokerProxy.Set(brokerClient);
                BrokerChanging?.Invoke(this, EventArgs.Empty);
                var switchingFromNull = oldBroker is NullBrokerClient;

                var sessions = _sessions.Values.ToList();
                if (sessions.Any()) {
                    var sessionsSwitched = await TrySwitchSessionsAsync(sessions, oldBroker, cancellationToken);
                    if (!sessionsSwitched) {
                        BrokerChangeFailed?.Invoke(this, EventArgs.Empty);
                        return false;
                    }
                }

                PrintBrokerInformation();
            } finally {
                lockToken.Reset();
            }

            OnBrokerConnected();
            BrokerChanged?.Invoke(this, new EventArgs());
            return true;
        }

        private async Task<bool> TrySwitchSessionsAsync(List<RSession> sessions, IBrokerClient oldBroker, CancellationToken cancellationToken) {
            // All sessions should participate in switch. If any of it didn't start, cancel the rest.
            var startTransactionTasks = sessions.Select(s => s.StartSwitchingBrokerAsync(cancellationToken)).ToList();
            try {
                await Task.WhenAll(startTransactionTasks);
            } catch (OperationCanceledException) {
                foreach (var task in startTransactionTasks.Where(t => t.Status == TaskStatus.RanToCompletion)) {
                    task.Result.Dispose();
                }
                var newBroker = _brokerProxy.Set(oldBroker);
                newBroker.Dispose();
                return false;
            }

            // Try switching
            var transactions = startTransactionTasks.Select(t => t.Result).ToList();
            try {
                await ConnectSessionsToNewBrokerAsync(transactions, oldBroker);
                OnBrokerDisconnected();
                await CompleteSwitchingBrokerAsync(transactions, oldBroker);
            } catch (Exception ex) {
                _callback.WriteConsole(Environment.NewLine + Resources.RSessionProvider_ConnectionFailed.FormatInvariant(ex.Message));
                return false;
            } finally {
                foreach (var transaction in transactions) {
                    transaction.Dispose();
                }
            }

            return true;
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
            //foreach (var name in a.Interpreters) {
            //    _callback.WriteConsole("\t" + name);
            //}
        }

        private async Task ConnectToNewBrokerAsync(IRSessionSwitchBrokerTransaction transaction) {
            await transaction.ConnectToNewBrokerAsync();
        }

        private async Task ConnectSessionsToNewBrokerAsync(List<IRSessionSwitchBrokerTransaction> transactions, IBrokerClient oldBroker) {
            try {
                await Task.WhenAll(transactions.Select(ConnectToNewBrokerAsync));
            } catch (Exception) {
                var newBroker = _brokerProxy.Set(oldBroker);
                newBroker.Dispose();
                throw;
            }
        }

        private async Task CompleteSwitchingBrokerAsync(List<IRSessionSwitchBrokerTransaction> transactions, IBrokerClient oldBroker) {
            try {
                await Task.WhenAll(transactions.Select(t => t.CompleteSwitchingBrokerAsync()));
            } catch (OperationCanceledException ex) when (!(ex is RHostDisconnectedException)) {
                _callback.WriteConsole(Resources.RSessionProvider_StartingSessionAfterSwitchingCanceled);
                oldBroker.Dispose();
                throw;
            } catch (Exception) {
                var newBroker = _brokerProxy.Set(oldBroker);
                newBroker.Dispose();
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