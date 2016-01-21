using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class CopyPlotAsBitmapCommand : PlotWindowCommand {
        public CopyPlotAsBitmapCommand() :
            base(RPackageCommandId.icmdCopyPlotAsBitmap) {
        }

        protected override void SetStatus() {
            Enabled = PlotHistory.ActivePlotIndex >= 0;
        }

        protected override void Handle() {
            PlotHistory.PlotContentProvider.CopyToClipboardAsBitmap();
        }
    }
}
