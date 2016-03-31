// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.DataInspect.DataImport;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Data {
    internal sealed class ImportDataSetTextFileCommand : PackageCommand {

        public ImportDataSetTextFileCommand(IRSession rSession) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdImportDatasetTextFile) {

            if (rSession == null) {
                throw new ArgumentNullException("rSession");
            }

            RSession = rSession;
        }

        protected override void SetStatus() {
            Enabled = RSession.IsHostRunning;
        }

        protected override void Handle() {
            base.Handle();

            string filePath = VsAppShell.Current.BrowseForFileOpen(IntPtr.Zero, "CSV file|*.csv");
            if (!string.IsNullOrEmpty(filePath)) {
                var dlg = new ImportDataWindow(filePath);
                dlg.ShowModal();
            }
        }

        private IRSession RSession { get; }
    }
}
