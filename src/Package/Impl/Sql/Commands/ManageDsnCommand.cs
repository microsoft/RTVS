// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Sql {
    internal sealed class ManageDsnCommand : SessionCommand {
        private readonly IApplicationShell _appShell;

        public ManageDsnCommand(IApplicationShell appShell, IRSession session) : 
            base(RPackageCommandId.icmdManageDsn, session) {
            _appShell = appShell;
        }

        protected override void Handle() {
            NativeMethods.SQLManageDataSources(_appShell.GetDialogOwnerWindow());
        }
    }
}
