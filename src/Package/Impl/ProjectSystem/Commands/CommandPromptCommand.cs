// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using Microsoft.Common.Core.OS;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring;
using Microsoft.Common.Core;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    internal abstract class CommandPromptCommand : ICommandGroupHandler {
        private readonly int _commandId;
        private readonly IProcessServices _ps;

        public CommandPromptCommand(int id, IProcessServices ps) {
            _commandId = id;
            _ps = ps;
        }

        public CommandStatusResult GetCommandStatus(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if (commandId == _commandId && nodes.IsSingleNodePath()) {
                return new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported);
            }
            return CommandStatusResult.Unhandled;
        }

        public bool TryHandleCommand(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            if (commandId == _commandId) {
                var path = nodes.GetSingleNodePath();
                if (!string.IsNullOrEmpty(path)) {
                    if (File.Exists(path)) {
                        path = Path.GetDirectoryName(path);
                    }

                    path = path.TrimTrailingSlash();
                    var psi = new ProcessStartInfo();
                    SetFlags(psi, path);
                    _ps.Start(psi);
                }
                return true;
            }
            return false;
        }

        protected virtual void SetFlags(ProcessStartInfo psi, string path) { }
    }
}
