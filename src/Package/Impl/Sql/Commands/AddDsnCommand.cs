// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Sql {
    internal sealed class AddDsnCommand : SessionCommand {
        private readonly IApplicationShell _appShell;

        public AddDsnCommand(IApplicationShell appShell, IRSession session) :
            base(RPackageCommandId.icmdAddDsn, session) {
            _appShell = appShell;
        }

        protected override void Handle() {
            NativeMethods.SQLConfigDataSource(_appShell.GetDialogOwnerWindow(), NativeMethods.RequestFlags.ODBC_ADD_DSN, "SQL Server", null);
        }
    }
}
