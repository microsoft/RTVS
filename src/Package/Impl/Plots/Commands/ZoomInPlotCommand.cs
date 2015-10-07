using System.ComponentModel.Design;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands
{
    public sealed class ZoomInPlotCommand : MenuCommand
    {
        public ZoomInPlotCommand() :
            base((sender, args) => new Handler().OnCommand(),
                 new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdZoomInPlot))
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
