using System.ComponentModel.Design;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace
{
    public sealed class LoadWorkspaceCommand : MenuCommand
    {
        public LoadWorkspaceCommand() :
            base((sender, args) => new Handler().OnCommand(),
                 new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdLoadWorkspace))
        {
        }

        class Handler
        {
            public void OnCommand()
            {
            }
        }
    }
}
