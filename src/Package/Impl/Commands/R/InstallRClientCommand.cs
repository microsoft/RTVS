// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.R.Interpreters;
using Microsoft.VisualStudio.R.Package.RClient;
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
            var path = MicrosoftRClient.GetRClientPath();
            if (!string.IsNullOrEmpty(path)) {
                _shell.ShowMessage(Resources.Message_RClientIsAlreadyInstalled, MessageButtons.OK);
            } else {
                var installer = _shell.ExportProvider.GetExportedValue<IMicrosoftRClientInstaller>();
                installer.LaunchRClientSetup(_shell);
            }
        }
    }
}
