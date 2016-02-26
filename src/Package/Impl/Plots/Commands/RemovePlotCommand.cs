using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class RemovePlotCommand : PlotWindowCommand {
        public RemovePlotCommand(IPlotHistory plotHistory) :
            base(plotHistory, RPackageCommandId.icmdRemovePlot) {
        }

        protected override void SetStatus() {
            Enabled = PlotHistory.PlotCount > 0;
        }

        protected override void Handle() {
            if (VsAppShell.Current.ShowMessage(Resources.DeletePlot, MessageButtons.YesNo) == MessageButtons.Yes) {
                PlotContentProvider.DoNotWait(PlotHistory.PlotContentProvider.RemoveCurrentPlotAsync());
            }
        }
    }
}
