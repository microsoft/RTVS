// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots.Implementation.Commands;

namespace Microsoft.R.Components.Plots {
    public class RPlotDeviceCommands {
        public RPlotDeviceCommands(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceVisualComponent visualComponent) {
            if (interactiveWorkflow == null) {
                throw new ArgumentNullException(nameof(interactiveWorkflow));
            }

            if (visualComponent == null) {
                throw new ArgumentNullException(nameof(visualComponent));
            }

            ActivatePlotDevice = new PlotDeviceActivateCommand(interactiveWorkflow, visualComponent);
            ExportAsImage = new PlotDeviceExportAsImageCommand(interactiveWorkflow, visualComponent);
            ExportAsPdf = new PlotDeviceExportAsPdfCommand(interactiveWorkflow, visualComponent);
            Cut = new PlotDeviceCutCopyCommand(interactiveWorkflow, visualComponent, cut: true);
            Copy = new PlotDeviceCutCopyCommand(interactiveWorkflow, visualComponent, cut: false);
            Paste = new PlotDevicePasteCommand(interactiveWorkflow, visualComponent);
            CopyAsBitmap = new PlotDeviceCopyAsBitmapCommand(interactiveWorkflow, visualComponent);
            CopyAsMetafile = new PlotDeviceCopyAsMetafileCommand(interactiveWorkflow, visualComponent);
            RemoveAllPlots = new PlotDeviceRemoveAllCommand(interactiveWorkflow, visualComponent);
            RemoveCurrentPlot = new PlotDeviceRemoveCurrentCommand(interactiveWorkflow, visualComponent);
            NextPlot = new PlotDeviceMoveNextCommand(interactiveWorkflow, visualComponent);
            PreviousPlot = new PlotDeviceMovePreviousCommand(interactiveWorkflow, visualComponent);
            EndLocator = new PlotDeviceEndLocatorCommand(interactiveWorkflow, visualComponent);
        }

        public IAsyncCommand ActivatePlotDevice { get; }
        public IAsyncCommand ExportAsImage { get; }
        public IAsyncCommand ExportAsPdf { get; }
        public IAsyncCommand Cut { get; }
        public IAsyncCommand Copy { get; }
        public IAsyncCommand Paste { get; }
        public IAsyncCommand CopyAsBitmap { get; }
        public IAsyncCommand CopyAsMetafile { get; }
        public IAsyncCommand RemoveAllPlots { get; }
        public IAsyncCommand RemoveCurrentPlot { get; }
        public IAsyncCommand NextPlot { get; }
        public IAsyncCommand PreviousPlot { get; }
        public IAsyncCommand EndLocator { get; }
    }
}
