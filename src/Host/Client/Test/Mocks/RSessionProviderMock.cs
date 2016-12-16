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
        public bool HasBroker { get; } = true;
        public bool IsConnected { get; } = true;
        public bool IsRunning { get; } = true;
        public IBrokerClient Broker { get; } = new NullBrokerClient();

        public IRSession GetOrCreate(Guid guid) {
            IRSession session;
            if (!_sessions.TryGetValue(guid, out session)) {
                session = new RSessionMock();
                _sessions[guid] = session;
            }
            return session;
        }

        public IEnumerable<IRSession> GetSessions() => _sessions.Values;

        public Task TestBrokerConnectionAsync(string name, string path, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(true);

        public Task<bool> TrySwitchBrokerAsync(string name, string path = null, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(true);

#pragma warning disable 67
        public event EventHandler BrokerChanging;
        public event EventHandler BrokerChangeFailed;
        public event EventHandler BrokerChanged;
        public event EventHandler<BrokerStateChangedEventArgs> BrokerStateChanged;
        public event EventHandler<HostLoadChangedEventArgs> HostLoadChanged;
        public event EventHandler SessionStateChanged;
#pragma warning restore
    }
}
