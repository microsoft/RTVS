using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;

namespace Microsoft.R.Host.Client.Session {
    [Export(typeof(IRSessionProvider))]
    public class RSessionProvider : IRSessionProvider {
        private int _sessionCounter;
        private readonly ConcurrentDictionary<Guid, IRSession> _sessions = new ConcurrentDictionary<Guid, IRSession>();

        public IRSession GetOrCreate(Guid guid, IRHostClientApp hostClientApp) {
            return _sessions.GetOrAdd(guid, id => new RSession(Interlocked.Increment(ref _sessionCounter), hostClientApp, () => DisposeSession(guid)));
        }

        public IEnumerable<IRSession> GetSessions() {
            return _sessions.Values;
        }
        
        public void Dispose() {
            foreach (var session in _sessions.Values) {
                session.Dispose();
            }
        }

        private void DisposeSession(Guid guid) {
            IRSession session;
            _sessions.TryRemove(guid, out session);
        }
    }
}