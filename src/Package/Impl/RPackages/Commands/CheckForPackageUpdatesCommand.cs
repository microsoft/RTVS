using System.ComponentModel.Design;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.RPackages.Commands {
    internal sealed class CheckForPackageUpdatesCommand : PackageCommand {
        public CheckForPackageUpdatesCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdCheckForPackageUpdates) {
        }

        internal override void SetStatus() {
            Enabled = false;
        }

        internal override void Handle() {
        }
    }
}
