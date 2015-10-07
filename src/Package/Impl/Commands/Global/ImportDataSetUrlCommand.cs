using System.ComponentModel.Design;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Commands.Global
{
    public sealed class ImportDataSetUrlCommand : MenuCommand
    {
        public ImportDataSetUrlCommand() :
            base((sender, args) => new Handler().OnCommand(),
                 new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdImportDatasetUrl))
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
