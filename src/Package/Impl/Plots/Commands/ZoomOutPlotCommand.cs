using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands
{
    internal sealed class ZoomOutPlotCommand : PackageCommand
    {
        public ZoomOutPlotCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdZoomOutPlot)
        {
        }
        protected override void SetStatus()
        {
            Enabled = false;
        }
    }
}
