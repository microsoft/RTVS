using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Microsoft.R.Host.Client.Session {
    [Export(typeof(IRSessionProvider))]
    public class RSessionProvider : IRSessionProvider {
        private readonly object _lock = new object();
        private readonly Dictionary<int, IRSession> _sessions = new Dictionary<int, IRSession>();

        public IRSession Create(int sessionId, IRHostClientApp hostClientApp) {
            IRSession session, oldCurrent;

            lock (_lock) {
                if (_sessions.TryGetValue(sessionId, out session)) {
                    return session;
                }

                session = new RSession(sessionId, hostClientApp);
                _sessions[sessionId] = session;

                oldCurrent = Current;
                if (Current == null) {
                    Current = session;
                }
            }

            if (oldCurrent != Current) {
                CurrentChanged?.Invoke(this, EventArgs.Empty);
            }

            return session;
        }

        public IReadOnlyDictionary<int, IRSession> GetSessions() {
            lock (_lock) {
                return new Dictionary<int, IRSession>(_sessions);
            }
        }

        public IRSession Current { get; private set; }

        public event EventHandler CurrentChanged;

        public void Dispose() {
            lock (_lock) {
                foreach (var session in _sessions.Values) {
                    session.Dispose();
                }

                _sessions.Clear();
            }
        }
    }
}