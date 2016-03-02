// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Debugger;
using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.Debugger {
    [Export(typeof(IDebugSessionProvider))]
    internal class RDebugSessionProvider : IDebugSessionProvider {
        private readonly Dictionary<IRSession, DebugSession> _debugSessions = new Dictionary<IRSession, DebugSession>();
        private readonly SemaphoreSlim _sem = new SemaphoreSlim(1, 1);

        public async Task<DebugSession> GetDebugSessionAsync(IRSession session, CancellationToken cancellationToken = default(CancellationToken)) {
            DebugSession debugSession;

            await _sem.WaitAsync(cancellationToken).ConfigureAwait(false);
            try {
                if (!_debugSessions.TryGetValue(session, out debugSession)) {
                    debugSession = new DebugSession(session);
                    await debugSession.InitializeAsync(cancellationToken).ConfigureAwait(false);
                    _debugSessions.Add(session, debugSession);
                }

                session.Disposed += Session_Disposed;
            } finally {
                _sem.Release();
            }

            return debugSession;
        }

        private void Session_Disposed(object sender, System.EventArgs e) {
            var session = (IRSession)sender;
            DebugSession debugSession;

            _sem.Wait();
            try {
                if (_debugSessions.TryGetValue(session, out debugSession)) {
                    session.Disposed -= Session_Disposed;
                    debugSession.Dispose();
                    _debugSessions.Remove(session);
                }
            } finally {
                _sem.Release();
            }
        }
    }
}
