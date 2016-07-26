// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Sql.Publish;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Sql {
    internal sealed class PublishSProcCommand : PackageCommand {
        private readonly ICoreShell _coreShell;
        private readonly IProjectSystemServices _pss;

        public PublishSProcCommand(ICoreShell coreShell, IProjectSystemServices pss) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdPublishSProc) {
            _coreShell = coreShell;
            _pss = pss;
        }

        protected override void SetStatus() {
            Visible = Supported = true;
            Enabled = _pss.GetSelectedProject<IVsHierarchy>() != null;
        }

        protected override void Handle() {
            var project = _pss.GetSelectedProject<IVsHierarchy>()?.GetDTEProject();
            if (project != null) {
                if (SqlPublishDialogViewModel.GetDatabaseProjectsInSolution(_pss).Count > 0) {
                    var rFiles = _pss.GetProjectFiles(project).Where(x => Path.GetExtension(x).EqualsIgnoreCase(".R"));
                    var sqlFiles = new HashSet<string>(_pss.GetProjectFiles(project).Where(x => Path.GetExtension(x).EqualsIgnoreCase(".sql")));
                    var sprocFiles = rFiles.Where(x =>
                                sqlFiles.Contains(x + ".sql", StringComparer.OrdinalIgnoreCase) &&
                                sqlFiles.Contains(x + ".sproc.sql", StringComparer.OrdinalIgnoreCase));
                    if (sprocFiles.FirstOrDefault() != null) {
                        var dlg = new SqlPublshDialog(_coreShell, _pss, sprocFiles, Path.GetDirectoryName(project.FullName));
                        dlg.ShowModal();
                    } else {
                        _coreShell.ShowErrorMessage(Resources.SqlPublishDialog_NoSProcFiles);
                    }
                } else {
                    _coreShell.ShowErrorMessage(Resources.SqlPublishDialog_NoDbProject);
                }
            }
        }
    }
}
