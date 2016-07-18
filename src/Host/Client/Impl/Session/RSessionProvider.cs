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

namespace Microsoft.R.Host.Client.Session {
    [Export(typeof(IRSessionProvider))]
    public class RSessionProvider : IRSessionProvider {
        private int _sessionCounter;
        private readonly ConcurrentDictionary<Guid, IRSession> _sessions = new ConcurrentDictionary<Guid, IRSession>();
        private readonly DisposeToken _disposeToken = DisposeToken.Create<RSessionProvider>();

        public IRSession GetOrCreate(Guid guid) {
            _disposeToken.ThrowIfDisposed();
            return _sessions.GetOrAdd(guid, id => new RSession(Interlocked.Increment(ref _sessionCounter), () => DisposeSession(guid)));
        }

        public IEnumerable<IRSession> GetSessions() {
            return _sessions.Values;
        }

        public async Task<IRSessionEvaluation> BeginEvaluationAsync(RHostStartupInfo startupInfo, CancellationToken cancellationToken = new CancellationToken()) {
            var session = GetOrCreate(Guid.NewGuid());
            cancellationToken.ThrowIfCancellationRequested();

            try {
                await session.StartHostAsync(new RHostStartupInfo {
                    Name = "IsolatedRHost" + session.Id,
                    RBasePath = startupInfo.RBasePath,
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
            IRSession session;
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