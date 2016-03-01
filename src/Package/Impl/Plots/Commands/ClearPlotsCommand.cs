using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class ClearPlotsCommand : PlotWindowCommand {
        public ClearPlotsCommand(IPlotHistory plotHistory) :
            base(plotHistory, RPackageCommandId.icmdClearPlots) {
        }

        internal override void SetStatus() {
            Enabled = PlotHistory.PlotCount > 0;
        }

        internal override void Handle() {
            if (VsAppShell.Current.ShowMessage(Resources.DeleteAllPlots, MessageButtons.YesNo) == MessageButtons.Yes) {
                PlotContentProvider.DoNotWait(PlotHistory.PlotContentProvider.ClearAllAsync());
            }
        }
    }
}
