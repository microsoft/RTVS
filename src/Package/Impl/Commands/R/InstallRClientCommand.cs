// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Install;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal sealed class InstallRClientCommand : PackageCommand {
        public InstallRClientCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdInstallRClient) {
        }

        protected override void SetStatus() {
            Enabled = true;
        }

        protected override void Handle() {
            RInstallation.LaunchRClientSetup();
        }
    }
}
