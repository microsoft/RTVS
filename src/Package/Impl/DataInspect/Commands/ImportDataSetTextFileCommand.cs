// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.DataInspect.DataImport;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Commands {
    internal sealed class ImportDataSetTextFileCommand : SessionCommand {
        private readonly IUIService _ui;

        public ImportDataSetTextFileCommand(ICoreShell shell, IRSession session) :
            base(session, RGuidList.RCmdSetGuid, RPackageCommandId.icmdImportDatasetTextFile) {
            _ui = shell.UI();
        }

        protected override void SetStatus() {
            Enabled = RSession.IsHostRunning && !RSession.IsRemote;
        }

        protected override void Handle() {
            base.Handle();

            string filePath = _ui.FileDialog.ShowOpenFileDialog(Resources.CsvFileFilter, title: Resources.ImportData_EnterTextFileTitle);

            if (!string.IsNullOrEmpty(filePath)) {
                var dlg = new ImportDataWindow(filePath, null);
                dlg.ShowModal();
            }
        }
    }
}
