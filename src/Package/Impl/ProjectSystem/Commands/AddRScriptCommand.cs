// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Utilities;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo("RTools")]
    [OrderPrecedence(205)]
    internal sealed class AddRScriptCommand : ICommandGroupHandler {
        public CommandStatusResult GetCommandStatus(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if (commandId == RPackageCommandId.icmdAddRScript) {
                return new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported);
            }
            return CommandStatusResult.Unhandled;
        }

        public bool TryHandleCommand(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            if (commandId == RPackageCommandId.icmdAddRScript) {
                var path = nodes.GetNodeFolderPath();
                if (!string.IsNullOrEmpty(path)) {
                    ProjectUtilities.AddNewItem("rscript", "Script", "r", path);
                }
                return true;
            }
            return false;
        }
    }
}
