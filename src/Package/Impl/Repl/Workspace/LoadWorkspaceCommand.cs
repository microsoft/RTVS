using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace
{
    internal sealed class LoadWorkspaceCommand : PackageCommand
    {
        public LoadWorkspaceCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdLoadWorkspace)
        {
        }

        protected override void SetStatus()
        {
            this.Enabled = ReplWindow.ReplWindowExists();
        }
    }
}
