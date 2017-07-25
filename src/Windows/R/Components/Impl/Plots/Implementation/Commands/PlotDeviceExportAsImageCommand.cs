// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotDeviceExportAsImageCommand : PlotDeviceCommand, IAsyncCommand {
        public PlotDeviceExportAsImageCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceVisualComponent visualComponent)
            : base(interactiveWorkflow, visualComponent) {
        }

        public CommandStatus Status {
            get {
                if (HasCurrentPlot && !IsInLocatorMode) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported;
            }
        }

        public async Task InvokeAsync() {
            IRPlotExportDialogs plotExportDialogs = (IRPlotExportDialogs)InteractiveWorkflow.Shell.FileDialog();
            ExportArguments exportImageArguments = new ExportArguments(VisualComponent.Device.PixelWidth, VisualComponent.Device.PixelHeight, VisualComponent.Device.Resolution);
            ExportImageParameters exportImageParameters = plotExportDialogs.ShowExportImageDialog(exportImageArguments, Resources.Plots_ExportAsImageFilter, null, Resources.Plots_ExportAsImageDialogTitle);
            if (!string.IsNullOrEmpty(exportImageParameters?.FilePath)) {
                string device = DeviceFromFileExtension(exportImageParameters.FilePath);
                if (!string.IsNullOrEmpty(device)) {
                    try {
                        await InteractiveWorkflow.Plots.ExportToBitmapAsync(
                            VisualComponent.ActivePlot,
                            device,
                            exportImageParameters.FilePath,
                            exportImageParameters.PixelWidth,
                            exportImageParameters.PixelHeight,
                            exportImageParameters.Resolution);
                        if(exportImageParameters.ViewPlot) {
                            var process = new ProcessServices();
                            process.Start(exportImageParameters.FilePath);
                        }
                    } catch (RPlotManagerException ex) {
                        InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
                    } catch (OperationCanceledException) {
                    }
                } else {
                    InteractiveWorkflow.Shell.ShowErrorMessage(string.Format(Resources.Plots_ExportUnsupportedImageFormat, Path.GetExtension(exportImageParameters.FilePath)));
                }
            }
        }

        private string DeviceFromFileExtension(string filePath) {
            switch (Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant()) {
                case "png":
                    return "png";
                case "bmp":
                    return "bmp";
                case "tif":
                case "tiff":
                    return "tiff";
                case "jpg":
                case "jpeg":
                    return "jpeg";
                default:
                    return string.Empty;
            }
        }
    }
}
