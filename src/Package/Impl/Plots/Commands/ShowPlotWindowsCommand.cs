using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands
{
    internal sealed class ShowPlotWindowsCommand : PackageCommand
    {
        public ShowPlotWindowsCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdShowPlotWindow)
        {
        }

        protected override void Handle()
        {
            // TODO: find ad show all windows
            ToolWindowUtilities.ShowWindowPane<PlotWindowPane>(0, true);
        }
    }
}
