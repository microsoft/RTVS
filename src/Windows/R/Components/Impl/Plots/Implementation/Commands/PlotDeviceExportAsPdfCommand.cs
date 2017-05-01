// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotDeviceExportAsPdfCommand : PlotDeviceCommand, IAsyncCommand {
        public PlotDeviceExportAsPdfCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceVisualComponent visualComponent)
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
            var filePath = fd.ShowSaveFileDialog(Resources.Plots_ExportAsPdfFilter, null, Resources.Plots_ExportAsPdfDialogTitle);
            if (!string.IsNullOrEmpty(filePath)) {
                try {
                    await InteractiveWorkflow.Plots.ExportToPdfAsync(
                        VisualComponent.ActivePlot,
                        filePath,
                        PixelsToInches(VisualComponent.Device.PixelWidth),
                        PixelsToInches(VisualComponent.Device.PixelHeight));
                } catch (RPlotManagerException ex) {
                    InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
                } catch (OperationCanceledException) {
                }
            }
        }

        private static double PixelsToInches(int pixels) {
            return pixels / 96.0;
        }
    }
}
