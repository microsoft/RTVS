using System.ComponentModel.Design;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands
{
    public sealed class SavePlotCommand : MenuCommand
    {
        public SavePlotCommand() :
            base((sender, args) => new Handler().OnCommand(),
                 new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSavePlot))
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
