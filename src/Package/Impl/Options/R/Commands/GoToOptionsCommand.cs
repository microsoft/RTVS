// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Options.R.Commands {
    public sealed class GoToOptionsCommand : System.ComponentModel.Design.MenuCommand {
        private static IServiceContainer _services;

        public GoToOptionsCommand(IServiceContainer services) :
            base(OnCommand, new System.ComponentModel.Design.CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdGoToRToolsOptions)) {
            _services = services;
        }

        public static void OnCommand(object sender, EventArgs args) {
            var shell = _services.GetService<IVsShell>(typeof(SVsShell));
            IVsPackage package;

            if (VSConstants.S_OK == shell.LoadPackage(RGuidList.RPackageGuid, out package)) {
                ((Microsoft.VisualStudio.Shell.Package)package).ShowOptionPage(typeof(RToolsOptionsPage));
            }
        }
    }
}
