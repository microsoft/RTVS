using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands
{
    internal sealed class FixPlotCommand : PackageCommand
    {
        public FixPlotCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdFixPlot)
        {
        }

        protected override void SetStatus()
        {
            Enabled = false;
        }
    }
}
