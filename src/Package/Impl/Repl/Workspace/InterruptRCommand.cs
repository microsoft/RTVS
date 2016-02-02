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
        private readonly IRInteractiveWorkflowOperations _operations;

        private volatile bool _enabled;

        public InterruptRCommand(IRInteractiveWorkflow interactiveWorkflow) : base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdInterruptR) {
            _interactiveWorkflow = interactiveWorkflow;
            _operations = interactiveWorkflow.Operations;
            _session = interactiveWorkflow.RSession;
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
            if (window != null && window.Container.IsOnScreen) {
                Visible = true;
                Enabled = _session.IsHostRunning && _enabled;
            } else {
                Visible = false;
            }
        }

        protected override void Handle() {
            if (_enabled) {
                _operations.ClearPendingInputs();
                _session.CancelAllAsync().DoNotWait();
                _enabled = false;
            }
        }
    }
}
