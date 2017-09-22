// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.LanguageServer.Threading;

namespace Microsoft.R.LanguageServer.Services {
    internal sealed class IdleTimeService : IIdleTimeService, IIdleTimeSource, IIdleTimeNotification, IDisposable {
        private const int IdleDelay = 100;
        private readonly Timer _timer;
        private readonly IMainThreadPriority _mainThread;
        private readonly object _lock = new object();
        private DateTime _lastActivityTime = DateTime.Now;

        public IdleTimeService(IServiceContainer services) {
            _timer = new Timer(OnTimer, this, 200, 50);
            _mainThread = services.GetService<IMainThreadPriority>();
        }

        private static void OnTimer(object state) => ((IdleTimeService)state).HandleIdle();

        private void HandleIdle() {
            lock (_lock) {
                if ((DateTime.Now - _lastActivityTime).TotalMilliseconds > IdleDelay) {
                    _mainThread.Post(() => Idle?.Invoke(this, EventArgs.Empty), ThreadPostPriority.IdleOnce);
                }
            }
        }

        #region IIdleTimeSource
        public void DoIdle() => _mainThread.Post(() => Idle?.Invoke(this, EventArgs.Empty));
        #endregion

        #region IIdleTimeNotification
        public void NotifyUserActivity() {
            lock (_lock) {
                _lastActivityTime = DateTime.Now;
                _mainThread.CancelIdle();
            }
        }
        #endregion

        public void Dispose() {
            _timer.Dispose();
            Closing?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs> Idle;
        public event EventHandler<EventArgs> Closing;
    }
}
