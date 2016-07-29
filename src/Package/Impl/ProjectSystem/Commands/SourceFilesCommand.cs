// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring;
using Microsoft.VisualStudio.R.Package.Commands;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif
#if VS15
using Microsoft.VisualStudio.ProjectSystem;
#endif

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class SourceFilesCommand : ICommandGroupHandler {
        private IRInteractiveWorkflowProvider _interactiveWorkflowProvider;

        [ImportingConstructor]
        public SourceFilesCommand(IRInteractiveWorkflowProvider interactiveWorkflowProvider)  {
            _interactiveWorkflowProvider = interactiveWorkflowProvider;
        }

        public CommandStatusResult GetCommandStatus(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if ((commandId == RPackageCommandId.icmdSourceSelectedFiles || commandId == RPackageCommandId.icmdSourceSelectedFilesWithEcho) && nodes.GetSelectedNodesPaths().Count() > 0) {
                foreach (var n in nodes) {
                    if (n.IsFolder || !Path.GetExtension(n.FilePath).EqualsIgnoreCase(".r")) { 
                        return CommandStatusResult.Unhandled;
                    }
                }
                return new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported);
            }
            return CommandStatusResult.Unhandled;
        }

        public bool TryHandleCommand(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            if (commandId == RPackageCommandId.icmdSourceSelectedFiles || commandId == RPackageCommandId.icmdSourceSelectedFilesWithEcho) {
                var rFiles = nodes.GetSelectedNodesPaths().Where(x =>
                               Path.GetExtension(x).EqualsIgnoreCase(".r") &&
                               File.Exists(x));
                bool echo = commandId == RPackageCommandId.icmdSourceSelectedFilesWithEcho;
                _interactiveWorkflowProvider.GetOrCreate().Operations.SourceFiles(rFiles, echo);
                return true;
            }
            return false;
        }
    }
}
