// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.DataInspect.DataImport;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Commands {
    internal sealed class ImportDataSetTextFileCommand : SessionCommand {
        private readonly IServiceContainer _services;

        public ImportDataSetTextFileCommand(IServiceContainer services, IRSession session) :
            base(session, RGuidList.RCmdSetGuid, RPackageCommandId.icmdImportDatasetTextFile) {
            _services = services;
        }

        protected override void SetStatus() {
            Enabled = RSession.IsHostRunning && !RSession.IsRemote;
        }

        protected override void Handle() {
            base.Handle();

            string filePath = _services.FileDialog().ShowOpenFileDialog(Resources.CsvFileFilter, title: Resources.ImportData_EnterTextFileTitle);

            if (!string.IsNullOrEmpty(filePath)) {
                var dlg = new ImportDataWindow(_services, filePath, null);
                dlg.ShowModal();
            }
        }
    }
}
