using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class CopyPlotAsMetafileCommand : PlotWindowCommand {
        public CopyPlotAsMetafileCommand() :
            base(RPackageCommandId.icmdCopyPlotAsMetafile) {
        }

        protected override void SetStatus() {
            Enabled = PlotHistory.ActivePlotIndex >= 0;
        }

        protected override void Handle() {
            PlotHistory.PlotContentProvider.CopyToClipboardAsMetafile();
        }
    }
}
