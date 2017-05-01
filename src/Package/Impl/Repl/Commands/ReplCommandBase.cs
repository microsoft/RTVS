// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal abstract class ReplCommandBase : PackageCommand {
        protected IRInteractiveWorkflowVisual Workflow { get; }

        public ReplCommandBase(IRInteractiveWorkflowVisual interactiveWorkflow, int id) :
            base(RGuidList.RCmdSetGuid, id) {
            Workflow = interactiveWorkflow;
        }

        protected override void SetStatus() {
            if (Workflow.ActiveWindow != null) {
                Visible = true;
                Enabled = true;
            } else {
                Visible = false;
                Enabled = false;
            }
        }

        protected override void Handle() {
            if (Workflow.ActiveWindow != null) {
                DoOperation();
            }
        }

        protected abstract void DoOperation();
    }
}
