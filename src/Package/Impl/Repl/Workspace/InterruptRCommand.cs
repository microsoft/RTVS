using System;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace {
    internal sealed class InterruptRCommand : PackageCommand {
        private readonly IRInteractiveSession _interactiveSession;
        private readonly IRSession _session;
        private volatile bool _enabled;

        public InterruptRCommand(IRInteractiveSession interactiveSession) : base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdInterruptR) {
            _interactiveSession = interactiveSession;
            _session = interactiveSession.RSession;
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
            if (ReplWindow.IsActive) {
                Visible = true;
                Enabled = _session.IsHostRunning && _enabled;
            } else {
                Visible = false;
            }
        }

        protected override void Handle() {
            if (_enabled) {
                _interactiveSession.ClearPendingInputs();
                _session.CancelAllAsync().DoNotWait();
                _enabled = false;
            }
        }
    }
}
