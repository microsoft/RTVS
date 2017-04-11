// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Repl.Debugger {
    internal abstract class DebuggerWrappedCommand: DebuggerCommand {
        private Guid _shellGroup;
        private uint _shellCmdId;

        protected DebuggerWrappedCommand(IRInteractiveWorkflowVisual interactiveWorkflow, int cmdId, Guid shellGroup, int shellCmdId, DebuggerCommandVisibility visibility)
            : base(interactiveWorkflow, cmdId, visibility) {
            _shellGroup = shellGroup;
            _shellCmdId = (uint)shellCmdId;
        }

        protected override void Handle() {
            Workflow.Shell.PostCommand(_shellGroup, (int)_shellCmdId);
        }
    }
}
