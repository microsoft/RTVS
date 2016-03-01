using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Data {
    internal sealed class ImportDataSetUrlCommand : PackageCommand {
        public ImportDataSetUrlCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdImportDatasetUrl) {
        }

        internal override void SetStatus() {
            Enabled = false; // ReplWindow.ReplWindowExists();
        }
    }
}
