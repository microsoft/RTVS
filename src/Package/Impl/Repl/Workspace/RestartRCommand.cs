using System;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace {
    internal sealed class RestartRCommand : PackageCommand {
        public RestartRCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRestartR) {
        }

        protected override void SetStatus() {
            if (ReplWindow.Current.IsActive) {
                Visible = true;
                Enabled = ReplWindow.ReplWindowExists();
            } else {
                Visible = false;
            }
        }

        protected override void Handle() {
            IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            Guid group = VSConstants.VsStd11;
            object o = null;
            shell.PostExecCommand(ref group, (uint)VSConstants.VSStd11CmdID.InteractiveSessionRestart, 0, ref o);
        }
    }
}
