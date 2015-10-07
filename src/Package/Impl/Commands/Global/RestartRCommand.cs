using System.ComponentModel.Design;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Commands.Global
{
    public sealed class RestartRCommand : MenuCommand
    {
        public RestartRCommand() :
            base((sender, args) => new Handler().OnCommand(),
                 new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRestartR))
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
