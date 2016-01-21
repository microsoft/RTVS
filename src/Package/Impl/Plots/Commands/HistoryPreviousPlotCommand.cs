using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class HistoryPreviousPlotCommand : PlotWindowCommand {
        public HistoryPreviousPlotCommand() :
            base(RPackageCommandId.icmdPrevPlot) {
        }
        protected override void SetStatus() {
            Enabled = PlotHistory.ActivePlotIndex > 0;
        }
        protected override void Handle() {
            PlotContentProvider.DoNotWait(PlotHistory.PlotContentProvider.PreviousPlotAsync());
        }
    }
}
