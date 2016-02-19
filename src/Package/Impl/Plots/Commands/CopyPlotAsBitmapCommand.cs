using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class CopyPlotAsBitmapCommand : PlotWindowCommand {
        public CopyPlotAsBitmapCommand(IPlotHistory plotHistory) :
            base(plotHistory, RPackageCommandId.icmdCopyPlotAsBitmap) {
        }

        protected override void SetStatus() {
            Enabled = PlotHistory.ActivePlotIndex >= 0;
        }

        protected override void Handle() {
            PlotHistory.PlotContentProvider.CopyToClipboardAsBitmap();
        }
    }
}
