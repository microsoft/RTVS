// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;

namespace Microsoft.R.LanguageServer.Services {
    internal sealed class IdleTimeService : IIdleTimeService, IIdleTimeSource, IIdleTimeNotification, IDisposable {
        private const int IdleDelay = 100;
        private readonly Timer _timer;
        private readonly IMainThread _mainThread;
        private DateTime _lastActivityTime = DateTime.Now;

        public IdleTimeService(IServiceContainer services) {
            _timer = new Timer(OnTimer, this, 200, 50);
            _mainThread = services.MainThread();
        }

        private static void OnTimer(object state) => ((IdleTimeService)state).HandleIdle();

        private void HandleIdle() {
            if ((DateTime.Now - _lastActivityTime).TotalMilliseconds > IdleDelay) {
                _mainThread.Post(() => Idle?.Invoke(this, EventArgs.Empty));
            }
        }

        #region IIdleTimeSource
        public void DoIdle() => _mainThread.Post(() => Idle?.Invoke(this, EventArgs.Empty));
        #endregion

        #region IIdleTimeNotification
        public void NotifyUserActivity() {
            _lastActivityTime = DateTime.Now;
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
