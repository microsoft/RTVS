using System;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace {
    internal sealed class InterruptRCommand : PackageCommand {
        private IReplWindow _replWindow;
        private readonly IRSession _session;
        private IVsDebugger _debugger;
        private volatile bool _enabled;

        public InterruptRCommand(IReplWindow replWindow, IRSessionProvider rSessionProvider, IVsDebugger debugger) : 
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdInterruptR) {
            _replWindow = replWindow;
            _debugger = debugger;
            _session = rSessionProvider.GetInteractiveWindowRSession();
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

        internal override void SetStatus() {
            DBGMODE[] mode = new DBGMODE[1];
            _debugger.GetMode(mode);

            if (_replWindow.IsActive) {
                Visible = true;
                Enabled = _session.IsHostRunning && _enabled && mode[0] != DBGMODE.DBGMODE_Break;
            } else {
                Visible = false;
                Enabled = false;
            }
        }

        internal override void Handle() {
            if (_enabled) {
                _replWindow.ClearPendingInputs();
                _session.CancelAllAsync().DoNotWait();
                _enabled = false;
            }
        }
    }
}
