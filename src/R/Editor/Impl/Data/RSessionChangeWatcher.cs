// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Editor.Data {
    public abstract class RSessionChangeWatcher {
        protected IRSessionProvider SessionProvider { get; }
        protected IRSession Session { get; private set; }

        protected RSessionChangeWatcher(IRSessionProvider sessionProvider) {
            SessionProvider = sessionProvider;
        }

        public void Initialize() {
            ConnectToSession();
        }

        private void ConnectToSession() {
            if (Session == null) {
                Session = SessionProvider.GetOrCreate(GuidList.InteractiveWindowRSessionGuid);
                Session.Mutated += OnSessionMutated;
                Session.Disposed += OnSessionDisposed;
                SessionMutated();
            }
        }

        private void OnSessionDisposed(object sender, EventArgs e) {
            if (Session != null) {
                Session.Mutated -= OnSessionMutated;
                Session.Disposed -= OnSessionDisposed;
                Session = null;
            }
        }

        private void OnSessionMutated(object sender, EventArgs e) {
            if (Session != null) {
                SessionMutated();
            }
        }

        protected abstract void SessionMutated();
    }
}
