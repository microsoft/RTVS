// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Shiny {
    internal sealed class StopShinyAppCommand : PackageCommand {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;

        public StopShinyAppCommand(IRInteractiveWorkflow interactiveWorkflow)
            : base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdStopShinyApp) {
            _interactiveWorkflow = interactiveWorkflow;
        }

        protected override void SetStatus() {
            Visible = true;
            Enabled = _interactiveWorkflow.RSession.IsHostRunning && RunShinyAppCommand.RunningTask != null;
        }

        protected override void Handle() {
            if (RunShinyAppCommand.RunningTask != null) {
                _interactiveWorkflow.RSession.CancelAllAsync().DoNotWait();
            }
        }
    }
}
