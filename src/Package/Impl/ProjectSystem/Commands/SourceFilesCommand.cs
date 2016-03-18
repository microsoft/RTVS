// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Repl.Commands;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo("RTools")]
    [OrderPrecedence(200)]
    internal sealed class SourceFilesCommand : ICommandGroupHandler {
        public CommandStatusResult GetCommandStatus(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if (commandId == RPackageCommandId.icmdSourceSelectedFiles && !nodes.IsFolder() && nodes.GetSelectedNodesPaths().Count() > 0) {
                return new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported);
            }
            return CommandStatusResult.Unhandled;
        }

        public bool TryHandleCommand(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            if (commandId == RPackageCommandId.icmdSourceSelectedFiles) {
                var rFiles = nodes.GetSelectedNodesPaths().Where(x =>
                               Path.GetExtension(x).Equals(".r", StringComparison.OrdinalIgnoreCase) &&
                               File.Exists(x));
                foreach (var filePath in rFiles) {
                    SourceFileHelper.SourceFile(filePath);
                }
                return true;
            }
            return false;
        }
    }
}
