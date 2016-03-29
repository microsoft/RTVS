// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.DataInspect.DataImport;
using Microsoft.VisualStudio.R.Package.DataInspect.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Data {
    internal sealed class ImportDataSetTextFileCommand : PackageCommand {

        private IRSession _rSession;

        public ImportDataSetTextFileCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdImportDatasetTextFile) {
        }

        protected override void SetStatus() {
            Enabled = RSession.IsHostRunning;
        }

        protected override void Handle() {
            base.Handle();

            var dlg = new ImportDataWindow();
            dlg.ShowModal();
        }

        private IRSession RSession {
            get {
                if (_rSession == null) {
                    _rSession = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate().RSession;
                }
                return _rSession;
            }
        }
    }
}
