using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class CopyPlotAsMetafileCommand : PlotWindowCommand {
        public CopyPlotAsMetafileCommand(IPlotHistory plotHistory) :
            base(plotHistory, RPackageCommandId.icmdCopyPlotAsMetafile) {
        }

        internal override void SetStatus() {
            Enabled = PlotHistory.ActivePlotIndex >= 0;
        }

        internal override void Handle() {
            PlotHistory.PlotContentProvider.CopyToClipboardAsMetafile();
        }
    }
}
