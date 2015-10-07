using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace
{
    internal sealed class SaveWorkspaceCommand : PackageCommand
    {
        public SaveWorkspaceCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSaveWorkspace)
        {
        }

        protected override void SetStatus()
        {
            this.Enabled = ReplWindow.ReplWindowExists();
        }
    }
}
