// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Options.R.Tools {
    public sealed class SetupRemoteCommand : MenuCommand {
        private const string _remoteSetupPage = "http://aka.ms/rtvs-remote";

        public SetupRemoteCommand() :
            base(OnCommand, new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSetupRemote)) {
        }

        public static void OnCommand(object sender, EventArgs args) {
            Process.Start(_remoteSetupPage);
        }
    }
}
