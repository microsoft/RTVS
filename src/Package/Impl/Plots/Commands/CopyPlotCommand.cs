using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands
{
    internal sealed class CopyPlotCommand : PackageCommand
    {
        public CopyPlotCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdCopyPlot)
        {
        }

        protected override void SetStatus()
        {
            Enabled = false;
        }

        protected override void Handle()
        {
        }
    }
}
