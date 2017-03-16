// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public sealed partial class VsAppShell {
        private IdleTimeSource _idleTimeSource;

        private void ConfigureIdleSource() {
            _idleTimeSource = new IdleTimeSource();
            _idleTimeSource.Idle += OnIdle;
            _idleTimeSource.ApplicationStarted += OnApplicationStarted;
            _idleTimeSource.ApplicationClosing += OnApplicationClosing;
        }

        #region IIdleTimeService
        /// <summary>
        /// Fires when host application enters idle state.
        /// </summary>
        public event EventHandler<EventArgs> Idle;
        #endregion

        #region IIdleTimeSource
        public void DoIdle() {
            Idle?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        void OnIdle(object sender, EventArgs args) {
            DoIdle();
        }

        private void OnApplicationStarted(object sender, EventArgs e) => _appConstants.Initialize();
        private void OnApplicationClosing(object sender, EventArgs e) => Terminating?.Invoke(this, EventArgs.Empty);
    }
}
