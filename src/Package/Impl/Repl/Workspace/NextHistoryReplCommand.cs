// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace {
    internal sealed class NextHistoryReplCommand : ReplCommandBase {
        public NextHistoryReplCommand(IRInteractiveWorkflow interactiveWorkflow) : 
            base(interactiveWorkflow, RPackageCommandId.icmdNextHistoryRepl) { }

        protected override void DoOperation() => Workflow.ActiveWindow.InteractiveWindow.Operations.HistoryNext();
    }
}
