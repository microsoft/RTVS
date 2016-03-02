// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.RPackages.Commands {
    internal sealed class InstallPackagesCommand : PackageCommand {
        public InstallPackagesCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdInstallPackages) {
        }

        protected override void SetStatus() {
            Enabled = false;
        }

        protected override void Handle() {
        }
    }
}
