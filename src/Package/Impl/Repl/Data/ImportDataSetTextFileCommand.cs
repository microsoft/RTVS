using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Data
{
    internal sealed class ImportDataSetTextFileCommand : PackageCommand
    {
        public ImportDataSetTextFileCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdImportDatasetTextFile)
        {
        }
        protected override void SetStatus()
        {
            this.Enabled = ReplWindow.ReplWindowExists();
        }
    }
}
