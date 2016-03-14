// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.DataInspect.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Data {
    internal sealed class ImportDataSetTextFileCommand : PackageCommand {

        private IVariableDataProvider _variableProvider;

        public ImportDataSetTextFileCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdImportDatasetTextFile) {
        }

        protected override void SetStatus() {
            Enabled = VariableDataProvider.Enabled;
        }

        protected override void Handle() {
            base.Handle();

            throw new System.NotImplementedException();
        }

        private IVariableDataProvider VariableDataProvider {
            get {
                if (_variableProvider == null) {
                    _variableProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IVariableDataProvider>();
                }
                return _variableProvider;
            }
        }
    }
}
