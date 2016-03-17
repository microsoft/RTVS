// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.R.Package.Utilities;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    internal abstract class AddItemCommand : ICommandGroupHandler {
        private UnconfiguredProject _unconfiguredProject;
        private int _commandId;
        private string _templateName;
        private string _fileName;
        private string _extension;

        public AddItemCommand(UnconfiguredProject project, int id, string templateName, string fileName, string extension) {
            _unconfiguredProject = project;
            _commandId = id;
            _templateName = templateName;
            _fileName = fileName;
            _extension = extension;
        }

        public CommandStatusResult GetCommandStatus(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if (commandId == _commandId) {
                return new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported);
            }
            return CommandStatusResult.Unhandled;
        }

        public bool TryHandleCommand(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            if (commandId == _commandId) {
                var path = GetSelectedFolderPath(nodes);
                if (!string.IsNullOrEmpty(path)) {
                    ProjectUtilities.AddNewItem(_templateName, _fileName, _extension, path);
                    return true;
                }
            }
            return false;
        }

        private string GetSelectedFolderPath(IImmutableSet<IProjectTree> nodes) {
            if (nodes.Count == 1) {
                var n = nodes.First();
                if(n.Root == n) {
                    return Path.GetDirectoryName(_unconfiguredProject.FullPath);
                }
                return nodes.GetNodeFolderPath();
            }
            return string.Empty;
        }
    }
}
