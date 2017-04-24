// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Repl.Debugger {
    // Identical to AttachDebugger, and only exists as a separate command so that it can be
    // given a different label for better fit in the "Debug" top-level menu.
    internal sealed class AttachToRInteractiveCommand : AttachDebuggerCommand {
        public AttachToRInteractiveCommand(IRInteractiveWorkflowVisual interactiveWorkflow)
            : base(interactiveWorkflow, RPackageCommandId.icmdAttachToRInteractive, DebuggerCommandVisibility.DesignMode) {
        }
    }
}
