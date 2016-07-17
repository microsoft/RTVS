// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Editor.Data {
    public abstract class RSessionChangeWatcher {
        protected IRInteractiveWorkflow Workflow { get; }
        protected IRSession Session { get; private set; }

        protected RSessionChangeWatcher(IRInteractiveWorkflowProvider workflowProvider) {
            Workflow = workflowProvider.GetOrCreate();
        }

        public void Initialize() {
            ConnectToSession();
        }

        private void ConnectToSession() {
            if (Session == null) {
                Session = Workflow.RSession;
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
