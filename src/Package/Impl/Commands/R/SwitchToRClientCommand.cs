// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Interpreters;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal sealed class SwitchToRClientCommand : PackageCommand {
        private readonly IConnectionManager _connectionManager;
        private readonly ICoreShell _shell;

        public SwitchToRClientCommand(IConnectionManager connectionManager, ICoreShell shell) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSwitchToRClient) {
            _connectionManager = connectionManager;
            _shell = shell;
        }

        protected override void SetStatus() {
            Enabled = true;
        }

        protected override void Handle() {
            var rClientPath = SqlRClientInstallation.GetRClientPath();
            if (string.IsNullOrEmpty(rClientPath)) {
                if (_shell.ShowMessage(Resources.Prompt_RClientNotInstalled, MessageButtons.YesNo) == MessageButtons.Yes) {
                    var installer = _shell.ExportProvider.GetExportedValue<IMicrosoftRClientInstaller>();
                    installer.LaunchRClientSetup(_shell);
                    return;
                }
            }

            var connection = _connectionManager.ActiveConnection;
            if(!connection.IsRemote && !string.IsNullOrEmpty(connection.Path) && connection.Path.EqualsIgnoreCase(rClientPath)) {
                _shell.ShowMessage(Resources.Message_RClientIsAlreadySet, MessageButtons.OK);
                return;
            }

            var mrc = _connectionManager.RecentConnections.FirstOrDefault(c => c.Name.Contains("Microsoft R Client"));
            if (mrc != null) {
                _connectionManager.ConnectAsync(connection).DoNotWait();
            }
        }
    }
}
