// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.Common.Core.OS;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.Common.Core;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class OpenContainingFolderCommand : ICommandGroupHandler {
        private readonly UnconfiguredProject _unconfiguredProject;
        private readonly IProcessServices _ps;

        [ImportingConstructor]
        public OpenContainingFolderCommand(UnconfiguredProject unconfiguredProject, [Import(AllowDefault = true)] IProcessServices ps) {
            _unconfiguredProject = unconfiguredProject;
            _ps = ps ?? new ProcessServices();
        }

        public CommandStatusResult GetCommandStatus(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if (commandId == RPackageCommandId.icmdOpenContainingFolder && nodes.Count == 1) {
                return new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported);
            }
            return CommandStatusResult.Unhandled;
        }

        public bool TryHandleCommand(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            if (commandId == RPackageCommandId.icmdOpenContainingFolder) {
                var path = nodes.GetSelectedFolderPath(_unconfiguredProject);
                if (!string.IsNullOrEmpty(path)) {
                    path = path.TrimTrailingSlash();
                    _ps.Start(Path.GetDirectoryName(path));
                }
                return true;
            }
            return false;
        }
    }
}
