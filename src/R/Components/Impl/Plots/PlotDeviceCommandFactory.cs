// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots.Implementation.Commands;
using Microsoft.R.Components.Plots.ViewModel;

namespace Microsoft.R.Components.Plots {
    public static class PlotDeviceCommandFactory {
        public static IAsyncCommand NewPlotDevice(IRInteractiveWorkflow interactiveWorkflow) {
            return new PlotDeviceNewCommand(interactiveWorkflow);
        }

        public static IAsyncCommand ActivatePlotDevice(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceViewModel viewModel) {
            return new PlotDeviceActivateCommand(interactiveWorkflow, viewModel);
        }

        public static IAsyncCommand ExportAsImage(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceViewModel viewModel) {
            return new PlotDeviceExportAsImageCommand(interactiveWorkflow, viewModel);
        }

        public static IAsyncCommand ExportAsPdf(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceViewModel viewModel) {
            return new PlotDeviceExportAsPdfCommand(interactiveWorkflow, viewModel);
        }

        public static IAsyncCommand CutCopy(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceViewModel viewModel, bool cut) {
            return new PlotDeviceCutCopyCommand(interactiveWorkflow, viewModel, cut);
        }

        public static IAsyncCommand Paste(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceViewModel viewModel) {
            return new PlotDevicePasteCommand(interactiveWorkflow, viewModel);
        }

        public static IAsyncCommand CopyAsBitmap(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceViewModel viewModel) {
            return new PlotDeviceCopyAsBitmapCommand(interactiveWorkflow, viewModel);
        }

        public static IAsyncCommand CopyAsMetafile(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceViewModel viewModel) {
            return new PlotDeviceCopyAsMetafileCommand(interactiveWorkflow, viewModel);
        }

        public static IAsyncCommand RemoveAllPlots(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceViewModel viewModel) {
            return new PlotDeviceRemoveAllCommand(interactiveWorkflow, viewModel);
        }

        public static IAsyncCommand RemoveCurrentPlot(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceViewModel viewModel) {
            return new PlotDeviceRemoveCurrentCommand(interactiveWorkflow, viewModel);
        }

        public static IAsyncCommand NextPlot(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceViewModel viewModel) {
            return new PlotDeviceMoveNextCommand(interactiveWorkflow, viewModel);
        }

        public static IAsyncCommand PreviousPlot(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceViewModel viewModel) {
            return new PlotDeviceMovePreviousCommand(interactiveWorkflow, viewModel);
        }

        public static IAsyncCommand EndLocator(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceViewModel viewModel) {
            return new PlotDeviceEndLocatorCommand(interactiveWorkflow, viewModel);
        }
    }
}
