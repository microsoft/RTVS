// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.Common.Core.Disposables;

namespace Microsoft.R.Host.Client.Session {
    [Export(typeof(IRSessionProvider))]
    public class RSessionProvider : IRSessionProvider {
        private int _sessionCounter;
        private readonly ConcurrentDictionary<Guid, IRSession> _sessions = new ConcurrentDictionary<Guid, IRSession>();
        private readonly DisposeToken _disposeToken = DisposeToken.Create<RSessionProvider>();

        public IRSession GetOrCreate(Guid guid) {
            _disposeToken.ThrowIfDisposed();
            return _sessions.GetOrAdd(guid, id => new RSession(Interlocked.Increment(ref _sessionCounter), () => DisposeSession(guid)));
        }

        public IEnumerable<IRSession> GetSessions() {
            return _sessions.Values;
        }
        
        public void Dispose() {
            if (!_disposeToken.TryMarkDisposed()) {
                return;
            }

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