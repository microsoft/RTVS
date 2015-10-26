using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace {
    internal sealed class RestartRCommand : PackageCommand {
        public RestartRCommand() :
            base(VSConstants.VsStd11, (int)VSConstants.VSStd11CmdID.InteractiveSessionRestart) {
        }

        protected override void SetStatus() {
            if (ReplWindow.Current.IsActive) {
                Visible = true;
                Enabled = ReplWindow.ReplWindowExists();
            } else {
                Visible = false;
            }
        }
    }
}
