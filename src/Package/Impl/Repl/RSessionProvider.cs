using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.R.Package.Repl;

namespace Microsoft.R.Host.Client
{
    public class RSessionProvider : IRSessionProvider
    {
        private readonly ConcurrentDictionary<int, IRSession> _sessions = new ConcurrentDictionary<int, IRSession>();

        public IRSession Create(int sessionId)
        {
            IRSession session = new RSession();

            if (!_sessions.TryAdd(sessionId, session))
            {
                Debug.Fail($"Session with id {sessionId} is created already");
                return _sessions[sessionId];
            }

            return session;
        }

        public IRSession Current => _sessions.Values.FirstOrDefault();

        public void Dispose()
        {
            foreach (var session in _sessions.Values)
            {
                session.Dispose();
            }

            _sessions.Clear();
        }
    }
}