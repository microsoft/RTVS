using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.RPackages.Commands {
    internal sealed class InstallPackagesCommand : PackageCommand {
        public InstallPackagesCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdInstallPackages) {
        }

        internal override void SetStatus() {
            Enabled = false;
        }

        internal override void Handle() {
        }
    }
}
