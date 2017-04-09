// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.Common.Core.UI;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Commands {
    internal sealed class DeleteAllVariablesCommand : SessionCommand {
        private readonly IRInteractiveWorkflow _workflow;

        public DeleteAllVariablesCommand(IRInteractiveWorkflow workflow) :
            base(workflow.RSession, RGuidList.RCmdSetGuid, RPackageCommandId.icmdDeleteAllVariables) {
            _workflow = workflow;
        }

        protected override void SetStatus() {
            var variableWindowPane = ToolWindowUtilities.FindWindowPane<VariableWindowPane>(0);
            // 'Delete all variables' button should be enabled only when the Global environment 
            // is selected in Variable Explorer.
            Enabled = (variableWindowPane?.IsGlobalREnvironment()) ?? false;
        }

        protected override void Handle() {
            var ui = _workflow.Shell.UI();
            if (MessageButtons.No == ui.ShowMessage(Resources.Warning_DeleteAllVariables, MessageButtons.YesNo)) {
                return;
            }
            try {
                RSession.ExecuteAsync("rm(list = ls(all = TRUE))").DoNotWait();
            } catch (RException ex) {
                ui.ShowErrorMessage(Resources.Error_UnableToDeleteVariable.FormatInvariant(ex.Message));
            } catch (ComponentBinaryMissingException ex) {
                ui.ShowErrorMessage(Resources.Error_UnableToDeleteVariable.FormatInvariant(ex.Message));
            } catch (OperationCanceledException) { }
        }
    }
}
