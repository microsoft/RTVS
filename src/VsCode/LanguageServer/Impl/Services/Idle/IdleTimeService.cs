// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.LanguageServer.Services {
    internal sealed class IdleTimeService : IIdleTimeService, IIdleTimeSource, IIdleTimeNotification, IDisposable {
        private const int IdleDelay = 100;
        private readonly Timer _timer;
        private DateTime _lastActivityTime = DateTime.Now;

        public IdleTimeService() {
            _timer = new Timer(OnTimer, this, 200, 50);
        }

        private static void OnTimer(object state) {
            var svc = (IdleTimeService)state;
            if((DateTime.Now - svc._lastActivityTime).TotalMilliseconds > IdleDelay) {
                svc.Idle?.Invoke(svc, EventArgs.Empty);
            }
        }

        #region IIdleTimeSource
        public void DoIdle() => Idle?.Invoke(this, EventArgs.Empty);
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
