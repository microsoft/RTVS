// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Host.Client.Session {
    public class RSessionProvider : IRSessionProvider, IRHostConnector {
        private readonly ConcurrentDictionary<Guid, RSession> _sessions = new ConcurrentDictionary<Guid, RSession>();
        private readonly DisposeToken _disposeToken = DisposeToken.Create<RSessionProvider>();
        private int _sessionCounter;

        private IRHostBrokerConnector BrokerConnector { get; } = new RHostBrokerConnector();

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

            BrokerConnector.Dispose();
        }

        private void DisposeSession(Guid guid) {
            RSession session;
            _sessions.TryRemove(guid, out session);
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
                _session.StopHostAsync().DoNotWait();
            }
        }

        public bool IsRemote => BrokerConnector.IsRemote;
        public Uri BrokerUri => BrokerConnector.BrokerUri;

        public Task<RHost> ConnectAsync(string name, IRCallbacks callbacks, string rCommandLineArguments = null, int timeout = 3000, CancellationToken cancellationToken = new CancellationToken())
            => BrokerConnector.ConnectAsync(name, callbacks, rCommandLineArguments, timeout, cancellationToken);

        public event EventHandler BrokerChanged {
            add { BrokerConnector.BrokerChanged += value; }
            remove { BrokerConnector.BrokerChanged -= value; }
        }

        public Task<bool> TrySwitchBroker(string name, string path = null) {
            path = path ?? new RInstallation().GetRInstallPath();

            Uri uri;
            if (!Uri.TryCreate(path, UriKind.Absolute, out uri)) {
                return Task.FromResult(false);
            }

            if (uri.IsFile) {
                BrokerConnector.SwitchToLocalBroker(name, uri.LocalPath);
            } else {
                BrokerConnector.SwitchToRemoteBroker(uri);
            }

            return Task.FromResult(true);
        }
    }
}