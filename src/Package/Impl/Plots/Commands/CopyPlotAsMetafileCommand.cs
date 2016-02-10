using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class CopyPlotAsMetafileCommand : PlotWindowCommand {
        public CopyPlotAsMetafileCommand(IPlotHistory plotHistory) :
            base(plotHistory, RPackageCommandId.icmdCopyPlotAsMetafile) {
        }

        protected override void SetStatus() {
            Enabled = PlotHistory.ActivePlotIndex >= 0;
        }

        protected override void Handle() {
            PlotHistory.PlotContentProvider.CopyToClipboardAsMetafile();
        }
    }
}
