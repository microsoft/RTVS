using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands
{
    internal sealed class ExportPlotCommand : PackageCommand
    {
        public ExportPlotCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdExportPlot)
        {
        }

        protected override void SetStatus()
        {
            Enabled = false;
        }
    }
}
