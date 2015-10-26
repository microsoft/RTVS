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
            _rSessionProvider.CurrentSessionChanged += OnCurrentSessionChanged;
        }

        private void OnCurrentSessionChanged(object sender, EventArgs e) {
            if (_session != null) {
                _session.BeforeRequest -= OnBeforeRequest;
                _session.AfterRequest -= OnAfterRequest;
            }

            _session = _rSessionProvider.Current;

            if (_session != null) {
                _session.BeforeRequest += OnBeforeRequest;
                _session.AfterRequest += OnAfterRequest;
            }
        }

        private void OnBeforeRequest(object sender, RRequestEventArgs e) {
            _enabled = (e.Contexts.Count != 1);
        }

        private void OnAfterRequest(object sender, RRequestEventArgs e) {
            _enabled = (e.Contexts.Count == 1); // top lever prompt
        }

        protected override void SetStatus() {
            if (ReplWindow.Current.IsActive) {
                Visible = true;
                Enabled = (_rSessionProvider.Current != null) && (_enabled || !RToolsSettings.Current.EscInterruptsCalculation);
            } else {
                Visible = false;
            }
        }

        protected override void Handle() {
            if (_enabled || !RToolsSettings.Current.EscInterruptsCalculation) {
                _rSessionProvider.Current?.CancelAllAsync().DoNotWait();
            }
        }
    }
}
