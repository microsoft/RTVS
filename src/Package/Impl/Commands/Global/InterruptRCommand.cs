using System.ComponentModel.Design;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Commands.Global
{
    public sealed class InterruptRCommand : MenuCommand
    {
        public InterruptRCommand() :
            base((sender, args) => new Handler().OnCommand(),
                 new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdInterruptR))
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
