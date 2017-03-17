// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.R.Interpreters;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal sealed class InstallRClientCommand : PackageCommand {
        private readonly ICoreShell _shell;

        public InstallRClientCommand(ICoreShell shell) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdInstallRClient) {
            _shell = shell;
        }

        protected override void SetStatus() {
            Enabled = true;
        }

        protected override void Handle() {
            var path = SqlRClientInstallation.GetRClientPath();
            if (!string.IsNullOrEmpty(path)) {
                var ui = _shell.Services.GetService<IUIServices>();
                ui.ShowMessage(Resources.Message_RClientIsAlreadyInstalled, MessageButtons.OK);
            } else {
                var installer = _shell.Services.GetService<IMicrosoftRClientInstaller>();
                installer.LaunchRClientSetup(_shell);
            }
        }
    }
}
