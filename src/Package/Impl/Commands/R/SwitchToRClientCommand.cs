// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Host.Client.Install;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Options.R;
using Microsoft.VisualStudio.R.Package.RClient;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal sealed class SwitchToRClientCommand : PackageCommand {
        private readonly ICoreShell _shell;

        public SwitchToRClientCommand(ICoreShell shell) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSwitchToRClient) {
            _shell = shell;
        }

        protected override void SetStatus() {
            Enabled = true;
        }

        protected override void Handle() {
            var rClientPath = MicrosoftRClient.GetRClientPath();
            if (string.IsNullOrEmpty(rClientPath)) {
                if (_shell.ShowMessage(Resources.Prompt_RClientNotInstalled, MessageButtons.YesNo) == MessageButtons.Yes) {
                    var installer = _shell.ExportProvider.GetExportedValue<IMicrosoftRClientInstaller>();
                    installer.LaunchRClientSetup(_shell);
                    return;
                }
            }

            var currentRPath = RToolsSettings.Current.RBasePath;
            if(!string.IsNullOrEmpty(currentRPath) && currentRPath.EqualsIgnoreCase(rClientPath)) {
                _shell.ShowMessage(Resources.Message_RClientIsAlreadySet, MessageButtons.OK);
                return;
            }

            RToolsSettings.Current.RBasePath = rClientPath;

            var settings = _shell.ExportProvider.GetExportedValue<ISettingsStorage>();
            settings.Persist();

            _shell.ShowMessage(Resources.RPathChangedRestartVS, MessageButtons.OK);
        }
    }
}
