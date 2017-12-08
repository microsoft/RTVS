// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal sealed class ResetReplCommand : ReplCommandBase {
        public ResetReplCommand(IRInteractiveWorkflowVisual interactiveWorkflow) : 
            base(interactiveWorkflow, RPackageCommandId.icmdResetRepl) { }

        protected override void DoOperation() =>
            // Remember the working directory
            Workflow.RSession.GetRWorkingDirectoryAsync().ContinueWith(async t => {
                var directory = t.Result;
                await Workflow.Operations.ResetAsync();
                await Workflow.RSession.HostStarted;
                await Workflow.RSession.SetWorkingDirectoryAsync(directory);
            }).DoNotWait();
    }
}
