// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.VisualStudio.ProjectSystem;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands
{
    [ExportCommandGroup("1496A755-94DE-11D0-8C3F-00C04FC2AAE2")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class ExcludeFromProjectCommand : ICommandGroupHandler
    {
        public CommandStatusResult GetCommandStatus(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if ((VSConstants.VSStd2KCmdID)commandId == VSConstants.VSStd2KCmdID.EXCLUDEFROMPROJECT && nodes != null && nodes.Count > 0) {
                return new CommandStatusResult(true, commandText, CommandStatus.NotSupported | CommandStatus.Invisible);
            }

            return CommandStatusResult.Unhandled;
        }

        public bool TryHandleCommand(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            return false;
        }
    }
}
