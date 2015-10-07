using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands
{
    internal sealed class ZoomInPlotCommand : PackageCommand
    {
        public ZoomInPlotCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdZoomInPlot)
        {
        }

        protected override void SetStatus()
        {
            Enabled = false;
        }
    }
}
