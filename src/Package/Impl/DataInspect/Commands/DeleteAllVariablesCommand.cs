// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.R.Package.Utilities;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Commands {
    internal sealed class DeleteAllVariablesCommand : SessionCommand {
        public DeleteAllVariablesCommand(IRSession session) :
            base(session, RGuidList.RCmdSetGuid, RPackageCommandId.icmdDeleteAllVariables) {
        }

        protected override void SetStatus() {
            var variableWindowPane = ToolWindowUtilities.FindWindowPane<VariableWindowPane>(0);
            // 'Delete all variables' button should be enabled only when the Global environment 
            // is selected in Variable Explorer.
            Enabled = (variableWindowPane?.IsGlobalREnvironment()) ?? false;
        }

        protected override void Handle() {
            if(MessageButtons.No == VsAppShell.Current.ShowMessage(Resources.Warning_DeleteAllVariables, MessageButtons.YesNo)) {
                return;
            }
            try {
                RSession.ExecuteAsync("rm(list = ls(all = TRUE))").DoNotWait();
            } catch (RException ex) {
                VsAppShell.Current.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToDeleteVariable, ex.Message));
            } catch (OperationCanceledException) { }
        }
    }
}
