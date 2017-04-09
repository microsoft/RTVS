// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.DataInspect.DataImport;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Commands {
    internal sealed class ImportDataSetUrlCommand : SessionCommand {
        private readonly IServiceContainer _services;

        public ImportDataSetUrlCommand(IServiceContainer services, IRSession session) :
            base(session, RGuidList.RCmdSetGuid, RPackageCommandId.icmdImportDatasetUrl) {
            _services = services;
        }

        protected override void SetStatus() {
            Enabled = RSession.IsHostRunning && !RSession.IsRemote;
        }

        protected override void Handle() {
            base.Handle();

            var dlg = new EnterUrl();
            dlg.ShowModal();

            if (!string.IsNullOrEmpty(dlg.DownloadFilePath)) {
                var importDlg = new ImportDataWindow(_services, dlg.DownloadFilePath, dlg.VariableName);
                importDlg.ShowModal();
            }

            dlg.DeleteTemporaryFile();
        }
    }
}
