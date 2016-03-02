// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace {
    internal sealed class InterruptRCommand : PackageCommand {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly IRSession _session;
        private readonly IDebuggerModeTracker _debuggerModeTracker;
        private volatile bool _enabled;

        public InterruptRCommand(IRInteractiveWorkflow interactiveWorkflow, IDebuggerModeTracker debuggerModeTracker)
            : base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdInterruptR) {

            _interactiveWorkflow = interactiveWorkflow;
            _session = interactiveWorkflow.RSession;
            _debuggerModeTracker = debuggerModeTracker;
            _session.Disconnected += OnDisconnected;
            _session.BeforeRequest += OnBeforeRequest;
            _session.AfterRequest += OnAfterRequest;
        }

        private void OnDisconnected(object sender, EventArgs e) {
            _enabled = false;
        }

        private void OnBeforeRequest(object sender, RRequestEventArgs e) {
            _enabled = e.Contexts.Count != 1; // Disable command only if prompt is in the top level
        }

        private void OnAfterRequest(object sender, RRequestEventArgs e) {
            _enabled = true;
        }

        protected override void SetStatus() {
            var window = _interactiveWorkflow.ActiveWindow;
            if (window != null) {
                Visible = true;
                Enabled = _session.IsHostRunning && _enabled && !_debuggerModeTracker.IsEnteredBreakMode;
            } else {
                Visible = false;
                Enabled = false;
            }
        }

        protected override void Handle() {
            if (_enabled) {
                _interactiveWorkflow.Operations.ClearPendingInputs();
                _session.CancelAllAsync().DoNotWait();
                _enabled = false;
            }
        }
    }
}
