// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Shiny {
    internal sealed class RunShinyAppCommand : PackageCommand {
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;

        public RunShinyAppCommand(IRInteractiveWorkflowVisual interactiveWorkflow)
            : base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRunShinyApp) {
            _interactiveWorkflow = interactiveWorkflow;
        }

        protected override void SetStatus() {
            Visible = true;
            Enabled = _interactiveWorkflow.ActiveWindow != null && !_interactiveWorkflow.Operations.IsShinyAppRunning;
        }

        protected override void Handle() {
            _interactiveWorkflow.Operations.TryRunShinyApp();
        }
    }
}
