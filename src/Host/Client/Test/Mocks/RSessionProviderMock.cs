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
        private readonly Dictionary<Guid, IRSession> _sessions = new Dictionary<Guid, IRSession>();

        public void Dispose() { }

        public event EventHandler BrokerChanging;
        public event EventHandler BrokerChangeFailed;
        public event EventHandler BrokerChanged;

        public IBrokerClient Broker { get; } = new NullBrokerClient();

        public IRSession GetOrCreate(Guid guid) {
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

        public Task<IRSessionEvaluation> BeginEvaluationAsync(RHostStartupInfo startupInfo, CancellationToken cancellationToken = new CancellationToken()) 
            => new RSessionMock().BeginEvaluationAsync(cancellationToken);

        public Task<bool> TestBrokerConnectionAsync(string name, string path) => Task.FromResult(true);

        public Task<bool> TrySwitchBrokerAsync(string name, string path = null) {
            return Task.FromResult(true);
        }

        public void DisplayBrokerInformation() { }
    }
}
