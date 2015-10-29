using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Data {
    internal sealed class ImportDataSetUrlCommand : PackageCommand {
        public ImportDataSetUrlCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdImportDatasetUrl) {
        }

        protected override void SetStatus() {
            this.Enabled = false; // ReplWindow.ReplWindowExists();
        }
    }
}
