using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.R.Debugger;
using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.Debugger {
    [Export(typeof(IDebugSessionProvider))]
    internal class RDebugSessionProvider : IDebugSessionProvider {
        private readonly Dictionary<IRSession, DebugSession> _debugSessions = new Dictionary<IRSession, DebugSession>();
        private readonly object _lock = new object();

        public DebugSession GetDebugSession(IRSession session) {
            DebugSession debugSession;

            lock (_lock) {
                if (!_debugSessions.TryGetValue(session, out debugSession)) {
                    debugSession = new DebugSession(session);
                    _debugSessions.Add(session, debugSession);
                }

                session.Disposed += Session_Disposed;
            }

            return debugSession;
        }

        private void Session_Disposed(object sender, System.EventArgs e) {
            var session = (IRSession)sender;
            DebugSession debugSession;

            lock (_lock) {
                if (_debugSessions.TryGetValue(session, out debugSession)) {
                    session.Disposed -= Session_Disposed;
                    debugSession.Dispose();
                    _debugSessions.Remove(session);
                }
            }
        }
    }
}
