using System;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace {
    internal sealed class InterruptRCommand : PackageCommand {
        private readonly IRSessionProvider _rSessionProvider;
        private IRSession _session;
        private volatile bool _enabled;

        public InterruptRCommand(IRSessionProvider rSessionProvider) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdInterruptR) {
            _rSessionProvider = rSessionProvider;
            _rSessionProvider.CurrentChanged += OnCurrentSessionChanged;
        }

        private void OnCurrentSessionChanged(object sender, EventArgs e) {
            if (_session != null) {
                _session.Disconnected -= OnDisconnected;
                _session.BeforeRequest -= OnBeforeRequest;
                _session.AfterRequest -= OnAfterRequest;
            }

            _session = _rSessionProvider.Current;

            if (_session != null) {
                _session.Disconnected += OnDisconnected;
                _session.BeforeRequest += OnBeforeRequest;
                _session.AfterRequest += OnAfterRequest;
            }
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
            if (ReplWindow.Current.IsActive) {
                Visible = true;
                Enabled = _rSessionProvider.Current != null && _enabled;
            } else {
                Visible = false;
            }
        }

        protected override void Handle() {
            var rSession = _rSessionProvider.Current;
            if (rSession != null && _enabled) {
                ReplWindow.Current.ClearPendingInputs();
                rSession.CancelAllAsync().DoNotWait();
                _enabled = false;
            }
        }
    }
}
