// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal abstract class SessionCommand : PackageCommand {
        protected IRInteractiveWorkflow Workflow { get; }

        public SessionCommand(int id, IRInteractiveWorkflow workflow) :
            base(RGuidList.RCmdSetGuid, id) {
            Workflow = workflow;
        }

        protected override void SetStatus() {
            Enabled = Workflow.RSession.IsHostRunning;
        }
    }
}
