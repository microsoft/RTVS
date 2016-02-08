using System;
using System.Collections.Generic;
using Microsoft.R.Host.Client.Test.Mocks;

namespace Microsoft.R.Host.Client.Mocks {
    public sealed class RSessionProviderMock : IRSessionProvider {
        private Dictionary<Guid, IRSession> _sessions = new Dictionary<Guid, IRSession>();

        public void Dispose() {
        }

        public IRSession GetOrCreate(Guid guid, IRHostClientApp hostClientApp) {
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
    }
}
