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
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Host.Client.Session {
    public class RSessionProvider : IRSessionProvider, IRHostConnector {
        private readonly IRSessionProviderCallback _callback;
        private readonly ConcurrentDictionary<Guid, RSession> _sessions = new ConcurrentDictionary<Guid, RSession>();
        private readonly DisposeToken _disposeToken = DisposeToken.Create<RSessionProvider>();
        private readonly SemaphoreSlim _connectorSwitchLock = new SemaphoreSlim(1, 1);

        private IRHostConnector _hostConnector;
        private int _sessionCounter;

        public bool IsRemote => _hostConnector.IsRemote;
        public string Name => _hostConnector.Name;
        public Uri BrokerUri => _hostConnector.BrokerUri;
        public string BrokerName => _hostConnector.Name;
        public event EventHandler BrokerChanged;

        public RSessionProvider(IRSessionProviderCallback callback = null) {
            _callback = callback ?? new NullRSessionProviderCallback();
            _hostConnector = new NullRHostConnector();
        }

        public IRSession GetOrCreate(Guid guid) {
            _disposeToken.ThrowIfDisposed();
            return _sessions.GetOrAdd(guid, id => new RSession(Interlocked.Increment(ref _sessionCounter), this, () => DisposeSession(guid)));
        }

        public IEnumerable<IRSession> GetSessions() {
            return _sessions.Values;
        }

        public async Task<IRSessionEvaluation> BeginEvaluationAsync(RHostStartupInfo startupInfo, CancellationToken cancellationToken = default(CancellationToken)) {
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

            _hostConnector.Dispose();
        }

        private void DisposeSession(Guid guid) {
            RSession session;
            _sessions.TryRemove(guid, out session);
        }

        public async Task<bool> TestBrokerConnectionAsync(string name, string path) {
            var сonnector = await CreateConnectorAsync(name, path);
            if (сonnector == null) {
                return false;
            }

            var callbacks = new NullRCallbacks();
            try {
                var rhost = await сonnector.ConnectAsync(nameof(TestBrokerConnectionAsync), callbacks);
                var rhostRunTask = rhost.Run();
                callbacks.SetReadConsoleInput("q()\n");
                await rhostRunTask;
                return true;
            } catch (RHostDisconnectedException) {
                return false;
            }
        }

        public async Task<bool> TrySwitchBrokerAsync(string name, string path = null) {
            await TaskUtilities.SwitchToBackgroundThread();

            var newConnector = await CreateConnectorAsync(name, path);
            if (newConnector == null) {
                return false;
            }

            // Connector switching shouldn't be concurrent
            try {
                await _connectorSwitchLock.WaitAsync();

                // First switch connector so that all new sessions are created for the new broker
                var oldConnector = Interlocked.Exchange(ref _hostConnector, newConnector);
                var switchingFromNull = oldConnector is NullRHostConnector;
                if (!switchingFromNull) { 
                    _callback.WriteConsole(Resources.RSessionProvider_StartSwitchingWorkspaceFormat.FormatInvariant(_hostConnector.Name, GetUriString(_hostConnector)));
                }

                var sessions = _sessions.Values.ToList();

                if (sessions.Any()) {
                    try {
                        _callback.WriteConsole(Resources.RSessionProvider_StartConnectingToWorkspaceFormat.FormatInvariant(sessions.Count));
                        await Task.WhenAll(sessions.Select(StartSwitchingBrokerAsync));
                        _callback.WriteConsole(Resources.RSessionProvider_RestartingSessionsFormat.FormatInvariant(sessions.Count));
                        await Task.WhenAll(sessions.Select(s => s.CompleteSwitchingBrokerAsync()));
                    } catch (Exception) {
                        _callback.WriteConsole(Resources.RSessionProvider_SwitchingWorkspaceFailed.FormatInvariant(oldConnector.Name, GetUriString(oldConnector)));
                        Interlocked.Exchange(ref _hostConnector, oldConnector);
                        foreach (var session in sessions) {
                            session.CancelSwitchingBroker();
                        }
                        return false;
                    }
                }

                if (!switchingFromNull) {
                    _callback.WriteConsole(Resources.RSessionProvider_SwitchingRWorkspaceCompleted);
                }
                oldConnector.Dispose();
            } finally {
                _connectorSwitchLock.Release();
            }

            BrokerChanged?.Invoke(this, new EventArgs());
            return true;
        }

        private async Task StartSwitchingBrokerAsync(RSession session) {
            try {
                await session.StartSwitchingBrokerAsync();
            } catch (RHostDisconnectedException ex) {
                _callback.WriteConsole(Resources.RSessionProvider_RestartingSessionFailed.FormatInvariant(_hostConnector.Name, _hostConnector.BrokerUri, ex.Message));
                throw;
            }
        }

        private async Task<IRHostConnector> CreateConnectorAsync(string name, string path) {
            path = path ?? new RInstallation().GetCompatibleEngines().FirstOrDefault()?.InstallPath;

            Uri uri;
            if (!Uri.TryCreate(path, UriKind.Absolute, out uri)) {
                return null;
            }

            if (uri.IsFile) {
                return new LocalRHostConnector(name, uri.LocalPath) as IRHostConnector;
            }

            var windowHandle = await _callback.GetApplicationWindowHandleAsync();
            return new RemoteRHostConnector(name, uri, windowHandle);
        }

        public Task<RHost> ConnectAsync(string name, IRCallbacks callbacks, string rCommandLineArguments, int timeout = 3000, CancellationToken cancellationToken = new CancellationToken())
            => _hostConnector.ConnectAsync(name, callbacks, rCommandLineArguments, timeout, cancellationToken);

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
                _session.StopHostAsync().DoNotWait();
            }
        }

        private static string GetUriString(IRHostConnector connector)
            => connector.BrokerUri.IsFile ? connector.BrokerUri.LocalPath : connector.BrokerUri.AbsoluteUri;
    }
}