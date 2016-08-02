// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Test.Mocks;

namespace Microsoft.R.Host.Client.Mocks {
    public sealed class RSessionProviderMock : IRSessionProvider {
        private Dictionary<Guid, IRSession> _sessions = new Dictionary<Guid, IRSession>();

        public void Dispose() {
        }

        public IRSession GetOrCreate(Guid guid, IRHostBrokerConnector brokerConnector) {
            IRSession session;
            if (!_sessions.TryGetValue(guid, out session)) {
                session = new RSessionMock();
                _sessions[guid] = session;
            }
            return session;
        }

        public IEnumerable<IRSession> GetSessions() {
            return _sessions.Values;
        }

        public Task<IRSessionEvaluation> BeginEvaluationAsync(IRHostBrokerConnector hostFactory, RHostStartupInfo startupInfo, CancellationToken cancellationToken = new CancellationToken()) 
            => new RSessionMock().BeginEvaluationAsync(cancellationToken);

        public Task RestartSessions(IRHostBrokerConnector brokerConnector) => Task.CompletedTask;
    }
}
