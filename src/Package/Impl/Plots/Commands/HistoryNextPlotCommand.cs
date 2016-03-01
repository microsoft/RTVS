using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class HistoryNextPlotCommand : PlotWindowCommand {
        public HistoryNextPlotCommand(IPlotHistory plotHistory) :
            base(plotHistory, RPackageCommandId.icmdNextPlot) {
        }

        internal override void SetStatus() {
            Enabled = PlotHistory.ActivePlotIndex >= 0 && PlotHistory.ActivePlotIndex < PlotHistory.PlotCount - 1;
        }

        internal override void Handle() {
            PlotContentProvider.DoNotWait(PlotHistory.PlotContentProvider.NextPlotAsync());
        }
    }
}
