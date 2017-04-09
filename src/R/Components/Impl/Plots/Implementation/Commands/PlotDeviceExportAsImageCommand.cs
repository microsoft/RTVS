// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
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
            var fd = InteractiveWorkflow.Shell.FileDialog();
            string filePath = fd.ShowSaveFileDialog(Resources.Plots_ExportAsImageFilter, null, Resources.Plots_ExportAsImageDialogTitle);
            if (!string.IsNullOrEmpty(filePath)) {
                string device = DeviceFromFileExtension(filePath);
                if (!string.IsNullOrEmpty(device)) {
                    try {
                        await InteractiveWorkflow.Plots.ExportToBitmapAsync(
                            VisualComponent.ActivePlot,
                            device,
                            filePath,
                            VisualComponent.Device.PixelWidth,
                            VisualComponent.Device.PixelHeight,
                            VisualComponent.Device.Resolution);
                    } catch (RPlotManagerException ex) {
                        InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
                    } catch (OperationCanceledException) {
                    }
                } else {
                    InteractiveWorkflow.Shell.ShowErrorMessage(string.Format(Resources.Plots_ExportUnsupportedImageFormat, Path.GetExtension(filePath)));
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
