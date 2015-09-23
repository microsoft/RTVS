using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.Repl.Session
{
    public class RSessionProvider : IRSessionProvider
    {
        private readonly ConcurrentDictionary<int, IRSession> _sessions = new ConcurrentDictionary<int, IRSession>();

        public IRSession Create(int sessionId)
        {
            IRSession session = new RSession();

            if (!_sessions.TryAdd(sessionId, session))
            {
                Debug.Fail(string.Format(CultureInfo.InvariantCulture, "Session with id {0} is created already", sessionId));
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