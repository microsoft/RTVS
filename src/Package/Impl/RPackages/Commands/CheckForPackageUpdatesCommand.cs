using System.ComponentModel.Design;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.RPackages.Commands
{
    public sealed class CheckForPackageUpdatesCommand : MenuCommand
    {
        public CheckForPackageUpdatesCommand() :
            base((sender, args) => new Handler().OnCommand(),
                 new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdCheckForPackageUpdates))
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
