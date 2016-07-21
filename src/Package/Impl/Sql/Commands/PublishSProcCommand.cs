// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.R.Package.Sql.Publish;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif

namespace Microsoft.VisualStudio.R.Package.Sql {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(Constants.RtvsProjectCapability)]
    internal sealed class PublishSProcCommand : ICommandGroupHandler {
        private readonly ICoreShell _coreShell;
        private readonly UnconfiguredProject _unconfiguredProject;
        private readonly IProjectSystemServices _pss;

        [ImportingConstructor]
        public PublishSProcCommand(ICoreShell coreShell, IProjectSystemServices pss, UnconfiguredProject unconfiguredProject) {
            _coreShell = coreShell;
            _unconfiguredProject = unconfiguredProject;
            _pss = pss;
        }

        public CommandStatusResult GetCommandStatus(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if (commandId == RPackageCommandId.icmdPublishSProc && nodes.Count == 1) {
                return new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported);
            }
            return CommandStatusResult.Unhandled;
        }

        public bool TryHandleCommand(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            if (commandId == RPackageCommandId.icmdPublishSProc) {
                var folder = nodes.GetSelectedFolderPath(_unconfiguredProject);
                if (!string.IsNullOrEmpty(folder)) {
                    var dlg = new SqlPublshDialog(_coreShell, _pss, folder);
                    dlg.ShowModal();
                    return true;
                }
            }
            return false;
        }
    }
}
