// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Sql {
    internal sealed class AddDsnCommand : SessionCommand {
        private readonly IApplicationShell _appShell;

        public AddDsnCommand(IApplicationShell appShell, IRSession session) :
            base(RPackageCommandId.icmdAddDsn, session) {
            _appShell = appShell;
        }

        protected override void Handle() {
            IntPtr vsWindow;
            var uiShell = _appShell.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            uiShell.GetDialogOwnerHwnd(out vsWindow);
            NativeMethods.SQLConfigDataSource(vsWindow, NativeMethods.RequestFlags.ODBC_ADD_DSN, "SQL Server", null);
        }
    }
}
