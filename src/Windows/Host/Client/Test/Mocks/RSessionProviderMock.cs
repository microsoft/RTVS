// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Security;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Test.Mocks;

namespace Microsoft.R.Host.Client.Mocks {
    public sealed class RSessionProviderMock : IRSessionProvider {
        private readonly Dictionary<string, IRSession> _sessions = new Dictionary<string, IRSession>();

        public void Dispose() => BeforeDisposed?.Invoke(this, EventArgs.Empty);
        public bool HasBroker { get; } = true;
        public bool IsConnected { get; } = true;
        public IBrokerClient Broker { get; } = new NullBrokerClient();

        public IRSession GetOrCreate(string sessionId) {
            IRSession session;
            if (!_sessions.TryGetValue(sessionId, out session)) {
                session = new RSessionMock();
                _sessions[sessionId] = session;
            }
            return session;
        }

        public IEnumerable<IRSession> GetSessions() => _sessions.Values;

        public Task TestBrokerConnectionAsync(string name, BrokerConnectionInfo connectionInfo, CancellationToken cancellationToken = default(CancellationToken)) => Task.CompletedTask;

        public Task<bool> TrySwitchBrokerAsync(string name, BrokerConnectionInfo connectionInfo, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(true);

        public Task RemoveBrokerAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.CompletedTask;

#pragma warning disable 67
        public event EventHandler BeforeDisposed;
        public event EventHandler BrokerChanging;
        public event EventHandler BrokerChangeFailed;
        public event EventHandler BrokerChanged;
        public event EventHandler<BrokerStateChangedEventArgs> BrokerStateChanged;
        public event EventHandler<HostLoadChangedEventArgs> HostLoadChanged;
#pragma warning restore
    }
}
