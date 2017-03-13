// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.VisualStudio.ProjectSystem;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    internal abstract class ProjectCommand : ICommandGroupHandler {
        private readonly int _id;
        private readonly Func<IImmutableSet<IProjectTree>, bool> _nodesCheck;

        public ProjectCommand(int id, Func<IImmutableSet<IProjectTree>, bool> nodesCheck = null) {
            _id = id;
            _nodesCheck = nodesCheck ?? new Func<IImmutableSet<IProjectTree>, bool>(
                (IImmutableSet<IProjectTree>  nodes) => {
                    return nodes != null && nodes.Count > 0;
                });
        }

        public CommandStatusResult GetCommandStatus(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if ((int)commandId == _id && _nodesCheck(nodes)) {
                return new CommandStatusResult(true, commandText, CommandStatus.NotSupported | CommandStatus.Invisible);
            }
            return CommandStatusResult.Unhandled;
        }

        public bool TryHandleCommand(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            return false;
        }
    }
}
