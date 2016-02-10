using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace {
    internal sealed class ResetReplCommand : PackageCommand {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;

        public ResetReplCommand(IRInteractiveWorkflow interactiveWorkflow) : 
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdResetRepl) {
            _interactiveWorkflow = interactiveWorkflow;
        }

        protected override void SetStatus() {
            if (_interactiveWorkflow.ActiveWindow != null) {
                Visible = true;
                Enabled = true;
            } else {
                Visible = false;
                Enabled = false;
            }
        }

        protected override void Handle() {
            if (_interactiveWorkflow.ActiveWindow != null) {
                _interactiveWorkflow.Operations.ResetAsync().DoNotWait();
            }
        }
    }
}
