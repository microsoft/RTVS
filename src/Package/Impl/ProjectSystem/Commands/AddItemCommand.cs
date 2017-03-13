// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    internal abstract class AddItemCommand : ICommandGroupHandler {
        private readonly UnconfiguredProject _unconfiguredProject;
        private readonly IProjectSystemServices _pss;
        private readonly int _commandId;
        private readonly string _templateName;
        private readonly string _fileName;
        private readonly string _extension;

        public AddItemCommand(UnconfiguredProject project, IProjectSystemServices pss, int id, string templateName, string fileName, string extension) {
            _unconfiguredProject = project;
            _pss = pss;
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
                var path = nodes.GetSelectedFolderPath(_unconfiguredProject);
                if (!string.IsNullOrEmpty(path)) {
                     _pss.AddNewItem(_templateName, _fileName, _extension, path);
                    return true;
                }
            }
            return false;
        }
    }
}
