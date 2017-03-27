// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Sql {
    internal sealed class AddDsnCommand : SessionCommand {
        private readonly ICoreShell _shell;

        public AddDsnCommand(ICoreShell shell, IRInteractiveWorkflow workflow) :
            base(RPackageCommandId.icmdAddDsn, workflow) {
            _shell = shell;
        }

        protected override void Handle() {
            NativeMethods.SQLConfigDataSource(_shell.GetDialogOwnerWindow(), NativeMethods.RequestFlags.ODBC_ADD_DSN, "SQL Server", null);
        }
    }
}
