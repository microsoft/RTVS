// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.DataInspect.DataImport;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Data {
    internal sealed class ImportDataSetUrlCommand : PackageCommand {
        public ImportDataSetUrlCommand(IRSession rSession) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdImportDatasetUrl) {

            if (rSession == null) {
                throw new ArgumentNullException("rSession");
            }

            RSession = rSession;
        }

        private IRSession RSession { get; }

        protected override void SetStatus() {
            Enabled = RSession.IsHostRunning;
        }

        protected override void Handle() {
            base.Handle();

            var dlg = new EnterUrl();
            dlg.ShowModal();

            if (!string.IsNullOrEmpty(dlg.DownloadFilePath)) {
                var importDlg = new ImportDataWindow(dlg.DownloadFilePath, dlg.VariableName);
                importDlg.ShowModal();
            }

            dlg.DeleteTemporaryFile();
        }
    }
}
