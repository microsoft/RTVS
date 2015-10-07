using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands
{
    internal sealed class PrintPlotCommand : PackageCommand
    {
        public PrintPlotCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdPrintPlot)
        {
        }

        protected override void SetStatus()
        {
            Enabled = false;
        }
    }
}
