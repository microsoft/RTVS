using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands
{
    internal sealed class SavePlotCommand : PackageCommand
    {
        public SavePlotCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSavePlot)
        {
        }
        protected override void SetStatus()
        {
            Enabled = false;
        }
    }
}
