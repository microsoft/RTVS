// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.DataInspect.DataImport;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Commands {
    internal sealed class ImportDataSetTextFileCommand : SessionCommand {
        public ImportDataSetTextFileCommand(IRSession session) :
            base(session, RGuidList.RCmdSetGuid, RPackageCommandId.icmdImportDatasetTextFile) {
        }

        protected override void Handle() {
            base.Handle();

            string filePath = VsAppShell.Current.BrowseForFileOpen(IntPtr.Zero, Package.Resources.CsvFileFilter);
            if (!string.IsNullOrEmpty(filePath)) {
                var dlg = new ImportDataWindow(filePath, null);
                dlg.ShowModal();
            }
        }
    }
}
