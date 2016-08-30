// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.R.Components.Controller;
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

        public async Task<CommandResult> InvokeAsync() {
            string filePath = InteractiveWorkflow.Shell.ShowSaveFileDialog(Resources.Plots_ExportAsImageFilter, null, Resources.Plots_ExportAsImageDialogTitle);
            if (!string.IsNullOrEmpty(filePath)) {
                string device = DeviceFromFileExtension(filePath);
                if (!string.IsNullOrEmpty(device)) {
                    try {
                        await VisualComponent.ExportToBitmapAsync(device, filePath);
                    } catch (RPlotManagerException ex) {
                        InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
                    } catch (OperationCanceledException) {
                    }
                } else {
                    InteractiveWorkflow.Shell.ShowErrorMessage(string.Format(Resources.Plots_ExportUnsupportedImageFormat, Path.GetExtension(filePath)));
                }
            }

            return CommandResult.Executed;
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
