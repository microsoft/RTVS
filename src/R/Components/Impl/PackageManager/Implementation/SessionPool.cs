// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.PackageManager.Implementation {
    internal sealed class SessionPool : IDisposable {
        private readonly List<IRSession> _freeSessions = new List<IRSession>();
        private readonly SemaphoreSlim _sem = new SemaphoreSlim(1, 1);
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRSettings _settings;
        private bool _disposed;

        private const int MaxFreeSessions = 1;

        /// <summary>
        /// Timeout to allow R-Host to start. Typically only needs
        /// different value in tests or code coverage runs.
        /// </summary>
        private const int HostStartTimeout = 3000;

        public SessionPool(IRSessionProvider sessionProvider, IRSettings settings) {
            _sessionProvider = sessionProvider;
            _settings = settings;
        }

        public async Task<SessionToken> GetSession() {
            if (_disposed) {
                throw new InvalidOperationException("Session pool is disposed");
            }

            await _sem.WaitAsync();
            try {
                return new SessionToken(this, await GetSessionInternal());
            } finally {
                _sem.Release();
            }
        }

        internal async Task ReleaseSession(IRSession session) {
            if (session == null) {
                return;
            }
            await _sem.WaitAsync();
            if (_disposed || _freeSessions.Count >= MaxFreeSessions || !session.IsHostRunning) {
                session.Dispose();
            } else {
                _freeSessions.Add(session);
            }
            _sem.Release();
        }

        public void Dispose() {
            foreach (var s in _freeSessions) {
                s.Dispose();
            }
            _freeSessions.Clear();
            _disposed = true;
        }

        private async Task<IRSession> GetSessionInternal() {
            IRSession session = null;
            while (_freeSessions.Count > 0) {
                var s = _freeSessions[0];
                _freeSessions.RemoveAt(0);

                if (s.IsHostRunning) {
                    session = s;
                    break;
                }
                s.Dispose();
            }
            return session ?? await CreatePackageQuerySessionAsync();
        }

        private async Task<IRSession> CreatePackageQuerySessionAsync() {
            var g = Guid.NewGuid();
            var session = _sessionProvider.GetOrCreate(g);
            if (!session.IsHostRunning) {
                await session.StartHostAsync(new RHostStartupInfo {
                    Name = "PkgMgr " + g.ToString(),
                    RBasePath = _settings.RBasePath,
                    CranMirrorName = _settings.CranMirror
                }, null, HostStartTimeout);
            }
            return session;
        }
    }

    internal sealed class SessionToken : IDisposable {
        private readonly SessionPool _pool;

        public SessionToken(SessionPool pool, IRSession session) {
            _pool = pool;
            Session = session;
        }

        public IRSession Session { get; }

        public void Dispose() {
            _pool.ReleaseSession(Session).DoNotWait();
        }
    }
}
