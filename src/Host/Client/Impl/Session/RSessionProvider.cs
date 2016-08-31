// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.R.Host.Client.Session {
    [Export(typeof(IRSessionProvider))]
    public class RSessionProvider : IRSessionProvider {
        private readonly ConcurrentDictionary<Guid, RSession> _sessions = new ConcurrentDictionary<Guid, RSession>();
        private readonly DisposeToken _disposeToken = DisposeToken.Create<RSessionProvider>();
        private int _sessionCounter;

        public IRSession GetOrCreate(Guid guid, IRHostBrokerConnector brokerConnector) {
            _disposeToken.ThrowIfDisposed();
            return _sessions.GetOrAdd(guid, id => new RSession(Interlocked.Increment(ref _sessionCounter), brokerConnector, () => DisposeSession(guid)));
        }

        public IEnumerable<IRSession> GetSessions() {
            return _sessions.Values;
        }

        public async Task<IRSessionEvaluation> BeginEvaluationAsync(IRHostBrokerConnector brokerConnector, RHostStartupInfo startupInfo, CancellationToken cancellationToken = default(CancellationToken)) {
            var session = GetOrCreate(Guid.NewGuid(), brokerConnector);
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
    }
}