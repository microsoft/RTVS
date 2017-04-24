// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal sealed class NextHistoryReplCommand : ReplCommandBase {
        private readonly IRHistory _history;

        public NextHistoryReplCommand(IRInteractiveWorkflowVisual interactiveWorkflow) :
            base(interactiveWorkflow, RPackageCommandId.icmdNextHistoryRepl) {
            _history = interactiveWorkflow.History;
        }

        protected override void DoOperation() => _history.NextEntry();
    }
}
