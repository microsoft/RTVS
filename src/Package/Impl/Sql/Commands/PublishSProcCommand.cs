// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Sql.Publish;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#else
using Microsoft.VisualStudio.ProjectSystem;
#endif

namespace Microsoft.VisualStudio.R.Package.Sql {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class PublishSProcCommand : ICommandGroupHandler {
        private readonly IApplicationShell _appShell;
        private readonly IProjectSystemServices _pss;

        [ImportingConstructor]
        public PublishSProcCommand(IApplicationShell appShell, IProjectSystemServices pss) {
            _appShell = appShell;
            _pss = pss;
        }

        public CommandStatusResult GetCommandStatus(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if (commandId == RPackageCommandId.icmdPublishSProc) {
                return new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported);
            }
            return CommandStatusResult.Unhandled;
        }

        public bool TryHandleCommand(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            if (commandId == RPackageCommandId.icmdPublishSProc) {
                Handle();
                return true;
            }
            return false;
        }

        private void Handle() {
            var project = _pss.GetSelectedProject<IVsHierarchy>()?.GetDTEProject();
            if (project != null) {
                if (SqlPublishDialogViewModel.GetDatabaseProjectsInSolution(_pss).Count > 0) {
                    var rFiles = _pss.GetProjectFiles(project).Where(x => Path.GetExtension(x).EqualsIgnoreCase(".R"));
                    var sqlFiles = new HashSet<string>(_pss.GetProjectFiles(project).Where(x => Path.GetExtension(x).EqualsIgnoreCase(".sql")));
                    var sprocFiles = rFiles.Where(x =>
                                sqlFiles.Contains(x.ToQueryFilePath(), StringComparer.OrdinalIgnoreCase) &&
                                sqlFiles.Contains(x.ToSProcFilePath(), StringComparer.OrdinalIgnoreCase));
                    if (sprocFiles.Any()) {
                        var dlg = new SqlPublshDialog(_appShell, _pss, sprocFiles);
                        dlg.ShowModal();
                    } else {
                        _appShell.ShowErrorMessage(Resources.SqlPublishDialog_NoSProcFiles);
                    }
                } else {
                    _appShell.ShowErrorMessage(Resources.SqlPublishDialog_NoDbProject);
                }
            }
        }
    }
}
