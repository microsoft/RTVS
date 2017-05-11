// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI;
using Microsoft.R.Interpreters;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal sealed class InstallRClientCommand : PackageCommand {
        private readonly IServiceContainer _services;

        public InstallRClientCommand(IServiceContainer services) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdInstallRClient) {
            _services = services;
        }

        protected override void SetStatus() {
            Enabled = true;
        }

        protected override void Handle() {
            var path = SqlRClientInstallation.GetRClientPath();
            if (!string.IsNullOrEmpty(path)) {
                _services.UI().ShowMessage(Resources.Message_RClientIsAlreadyInstalled, MessageButtons.OK);
            } else {
                var installer = _services.GetService<IMicrosoftRClientInstaller>();
                installer.LaunchRClientSetup(_services);
            }
        }
    }
}
