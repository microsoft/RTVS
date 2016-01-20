using System;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class ExportPlotAsPdfCommand : PlotWindowCommand {
        public ExportPlotAsPdfCommand() :
            base(RPackageCommandId.icmdExportPlotAsPdf) {
        }

        protected override void SetStatus() {
            Enabled = PlotHistory.ActivePlotIndex >= 0;
        }

        protected override void Handle() {
            string destinationFilePath = VsAppShell.Current.BrowseForFileSave(IntPtr.Zero, Resources.PlotExportAsPdfFilter, null, Resources.ExportPlotAsPdfDialogTitle);
            if (!string.IsNullOrEmpty(destinationFilePath)) {
                PlotHistory.PlotContentProvider.ExportAsPdf(destinationFilePath);
            }
        }
    }
}
