// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Commands {
    internal sealed class DeleteAllVariablesCommand : SessionCommand {
        public DeleteAllVariablesCommand(IRSession session) :
            base(session, RGuidList.RCmdSetGuid, RPackageCommandId.icmdDeleteAllVariables) {
        }

        protected override void Handle() {
            if(MessageButtons.No == VsAppShell.Current.ShowMessage(Resources.Warning_DeleteAllVariables, MessageButtons.YesNo)) {
                return;
            }
            RSession.EvaluateAsync("rm(list = ls(all = TRUE))", REvaluationKind.Mutating).DoNotWait();
        }
    }
}
