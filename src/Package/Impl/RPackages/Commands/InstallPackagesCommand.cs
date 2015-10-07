using System.ComponentModel.Design;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.RPackages.Commands
{
    public sealed class InstallPackagesCommand : MenuCommand
    {
        public InstallPackagesCommand() :
            base((sender, args) => new Handler().OnCommand(),
                 new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdInstallPackages))
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
