// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Packages.R;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class SetDirectoryHereCommand : ICommandGroupHandler {
        private readonly IRInteractiveWorkflowProvider _interactiveWorkflowProvider;
        private readonly UnconfiguredProject _unconfiguredProject;

        [ImportingConstructor]
        public SetDirectoryHereCommand(UnconfiguredProject unconfiguredProject, IRInteractiveWorkflowProvider interactiveWorkflowProvider) {
            _unconfiguredProject = unconfiguredProject;
            _interactiveWorkflowProvider = interactiveWorkflowProvider;
        }

        public CommandStatusResult GetCommandStatus(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if (commandId == RPackageCommandId.icmdSetDirectoryHere && nodes.Count == 1) {
                return new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported);
            }
            return CommandStatusResult.Unhandled;
        }

        public bool TryHandleCommand(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            if (commandId == RPackageCommandId.icmdSetDirectoryHere) {
                var path = nodes.GetSelectedFolderPath(_unconfiguredProject);
                if (!string.IsNullOrEmpty(path)) {
                    var o = new object();

                    var interactiveWorkflow = _interactiveWorkflowProvider.GetOrCreate();
                    var controller = ReplCommandController.FromTextView(interactiveWorkflow.ActiveWindow.TextView);

                    controller.Invoke(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSetWorkingDirectory, path, ref o);
                    return true;
                }
            }
            return false;
        }
    }
}
