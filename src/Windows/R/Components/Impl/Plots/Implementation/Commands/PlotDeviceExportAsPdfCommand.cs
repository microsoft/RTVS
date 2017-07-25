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

        public CommandStatus Status
            => HasCurrentPlot && !IsInLocatorMode ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;

        public async Task InvokeAsync() {
            var plotExportDialogs = InteractiveWorkflow.Shell.GetService<IRPlotExportDialogs>();
            var exportPdfArguments = new ExportArguments(VisualComponent.Device.PixelWidth, VisualComponent.Device.PixelHeight, VisualComponent.Device.Resolution);
            var exportPdfParameters = plotExportDialogs.ShowExportPdfDialog(exportPdfArguments, Resources.Plots_ExportAsPdfFilter, null, Resources.Plots_ExportAsPdfDialogTitle);
           
            if (!string.IsNullOrEmpty(exportPdfParameters?.FilePath)) {
                try {
                    await InteractiveWorkflow.Plots.ExportToPdfAsync(
                        VisualComponent.ActivePlot,
                        exportPdfParameters.RInternalPdfDevice,
                        exportPdfParameters.RInternalPaperName,
                        exportPdfParameters.FilePath,
                        exportPdfParameters.WidthInInches,
                        exportPdfParameters.HeightInInches);
                    if(exportPdfParameters.ViewPlot) {
                        InteractiveWorkflow.Shell.Process().Start(exportPdfParameters.FilePath);
                    }
                } catch (RPlotManagerException ex) {
                    InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
                } catch (OperationCanceledException) {
                }
            }
        }
    }
}
