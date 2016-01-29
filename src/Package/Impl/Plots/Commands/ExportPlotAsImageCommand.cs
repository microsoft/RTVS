using System;
using System.IO;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class ExportPlotAsImageCommand : PlotWindowCommand {
        public ExportPlotAsImageCommand(IPlotHistory plotHistory) :
            base(plotHistory, RPackageCommandId.icmdExportPlotAsImage) {
        }

        internal override void SetStatus() {
            Enabled = PlotHistory.ActivePlotIndex >= 0;
        }

        internal override void Handle() {
            string destinationFilePath = VsAppShell.Current.BrowseForFileSave(IntPtr.Zero, Resources.PlotExportAsImageFilter, null, Resources.ExportPlotAsImageDialogTitle);
            if (!string.IsNullOrEmpty(destinationFilePath)) {
                string device = String.Empty;
                string extension = Path.GetExtension(destinationFilePath).TrimStart('.').ToLowerInvariant();
                switch (extension) {
                    case "png":
                        device = "png";
                        break;
                    case "bmp":
                        device = "bmp";
                        break;
                    case "tif":
                    case "tiff":
                        device = "tiff";
                        break;
                    case "jpg":
                    case "jpeg":
                        device = "jpeg";
                        break;
                    default:
                        VsAppShell.Current.ShowErrorMessage(string.Format(Resources.PlotExportUnsupportedImageFormat, extension));
                        return;
                }

                PlotHistory.PlotContentProvider.ExportAsImage(destinationFilePath, device);
            }
        }
    }
}
