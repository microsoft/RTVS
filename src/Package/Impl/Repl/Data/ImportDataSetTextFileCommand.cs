using System.ComponentModel.Design;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Data
{
    public sealed class ImportDataSetTextFileCommand : MenuCommand
    {
        public ImportDataSetTextFileCommand() :
            base((sender, args) => new Handler().OnCommand(),
                 new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdImportDatasetTextFile))
        {
        }

        class Handler
        {
            public void OnCommand()
            {
            }
        }
    }
}
