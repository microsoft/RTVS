// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using Microsoft.R.Components.Plots;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class ExportPlotAsImageCommand : PlotWindowCommand {
        private readonly IApplicationShell _appShell;

        public ExportPlotAsImageCommand(IApplicationShell appShell, IPlotHistory plotHistory) :
            base(plotHistory, RPackageCommandId.icmdExportPlotAsImage) {
            _appShell = appShell;
        }

        protected override void SetStatus() {
            Enabled = PlotHistory.ActivePlotIndex >= 0 && !IsInLocatorMode;
        }

        protected override void Handle() {
            string destinationFilePath = _appShell.BrowseForFileSave(IntPtr.Zero, Resources.PlotExportAsImageFilter, null, Resources.ExportPlotAsImageDialogTitle);
            if (!string.IsNullOrEmpty(destinationFilePath)) {
                string device = string.Empty;
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
                        VsAppShell.Current.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.PlotExportUnsupportedImageFormat, extension));
                        return;
                }

                PlotHistory.PlotContentProvider.ExportAsImage(destinationFilePath, device);
            }
        }
    }
}
