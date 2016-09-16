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
        private readonly SemaphoreSlim _brokerSwitchLock = new SemaphoreSlim(1, 1);
        private readonly AsyncCountdownEvent _connectCde = new AsyncCountdownEvent(0);

        private int _sessionCounter;
        private readonly BrokerClientProxy _brokerProxy;
        public IBrokerClient Broker => _brokerProxy;

        public event EventHandler BrokerChanging;
        public event EventHandler BrokerChangeFailed;
        public event EventHandler BrokerChanged;

        public RSessionProvider(IRSessionProviderCallback callback = null) {
            _callback = callback ?? new NullRSessionProviderCallback();
            _brokerProxy = new BrokerClientProxy(_connectCde);
        }

        public IRSession GetOrCreate(Guid guid) {
            _disposeToken.ThrowIfDisposed();
            return _sessions.GetOrAdd(guid, id => new RSession(Interlocked.Increment(ref _sessionCounter), Broker, () => DisposeSession(guid)));
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

            Broker.Dispose();
        }

        private void DisposeSession(Guid guid) {
            RSession session;
            _sessions.TryRemove(guid, out session);
        }

        public async Task<bool> TestBrokerConnectionAsync(string name, string path) {
            var сonnector = await CreateBrokerClientAsync(name, path);
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

            var brokerClient = await CreateBrokerClientAsync(name, path);
            if (brokerClient == null) {
                return false;
            }

            if (brokerClient.Name.EqualsOrdinal(_brokerProxy.Name) && brokerClient.Uri.AbsoluteUri.PathEquals(_brokerProxy.Uri.AbsoluteUri)) {
                // Switching to the broker that is currently running is always successful
                return true;
            }

            // Connector switching shouldn't be concurrent
            try {
                await _brokerSwitchLock.WaitAsync();
                await _connectCde.WaitAsync();

                // First switch connector so that all new sessions are created for the new broker
                var oldBroker = _brokerProxy.Set(brokerClient);
                var switchingFromNull = oldBroker is NullBrokerClient;
                if (!switchingFromNull) {
                    _callback.WriteConsole(Resources.RSessionProvider_StartSwitchingWorkspaceFormat.FormatInvariant(_brokerProxy.Name, GetUriString(_brokerProxy)));
                }

                var sessions = _sessions.Values.ToList();

                if (sessions.Any()) {
                    BrokerChanging?.Invoke(this, EventArgs.Empty);

                    try {
                        _callback.WriteConsole(Resources.RSessionProvider_StartConnectingToWorkspaceFormat.FormatInvariant(sessions.Count));
                        await Task.WhenAll(sessions.Select(StartSwitchingBrokerAsync));
                        _callback.WriteConsole(Resources.RSessionProvider_RestartingSessionsFormat.FormatInvariant(sessions.Count));
                        await Task.WhenAll(sessions.Select(s => s.CompleteSwitchingBrokerAsync()));
                    } catch (Exception) {
                        _callback.WriteConsole(Resources.RSessionProvider_SwitchingWorkspaceFailed.FormatInvariant(oldBroker.Name, GetUriString(oldBroker)));
                        _brokerProxy.Set(oldBroker);
                        foreach (var session in sessions) {
                            session.CancelSwitchingBroker();
                        }
                        BrokerChangeFailed?.Invoke(this, EventArgs.Empty);
                        return false;
                    }
                }

                if (!switchingFromNull) {
                    _callback.WriteConsole(Resources.RSessionProvider_SwitchingRWorkspaceCompleted);
                }
                PrintBrokerInformation();
                oldBroker.Dispose();
            } finally {
                _brokerSwitchLock.Release();
            }

            BrokerChanged?.Invoke(this, new EventArgs());
            return true;
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

        private async Task StartSwitchingBrokerAsync(RSession session) {
            try {
                await session.StartSwitchingBrokerAsync();
            } catch (RHostDisconnectedException ex) {
                _callback.WriteConsole(Resources.RSessionProvider_RestartingSessionFailed.FormatInvariant(_brokerProxy.Name, _brokerProxy.Uri, ex.Message));
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