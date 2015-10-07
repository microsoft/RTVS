using System.ComponentModel.Design;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Commands.Global
{
    public sealed class SaveWorkspaceCommand : MenuCommand
    {
        public SaveWorkspaceCommand() :
            base((sender, args) => new Handler().OnCommand(),
                 new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSaveWorkspace))
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
