// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots.Implementation.Commands;

namespace Microsoft.R.Components.Plots {
    public class RPlotDeviceCommands {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly IRPlotDeviceVisualComponent _visualComponent;

        public RPlotDeviceCommands(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceVisualComponent visualComponent) {
            if (interactiveWorkflow == null) {
                throw new ArgumentNullException(nameof(interactiveWorkflow));
            }

            if (visualComponent == null) {
                throw new ArgumentNullException(nameof(visualComponent));
            }

            _interactiveWorkflow = interactiveWorkflow;
            _visualComponent = visualComponent;

            ActivatePlotDevice = new PlotDeviceActivateCommand(_interactiveWorkflow, _visualComponent);
            ExportAsImage = new PlotDeviceExportAsImageCommand(_interactiveWorkflow, _visualComponent);
            ExportAsPdf = new PlotDeviceExportAsPdfCommand(_interactiveWorkflow, _visualComponent);
            Cut = new PlotDeviceCutCopyCommand(_interactiveWorkflow, _visualComponent, cut: true);
            Copy = new PlotDeviceCutCopyCommand(_interactiveWorkflow, _visualComponent, cut: false);
            Paste = new PlotDevicePasteCommand(_interactiveWorkflow, _visualComponent);
            CopyAsBitmap = new PlotDeviceCopyAsBitmapCommand(_interactiveWorkflow, _visualComponent);
            CopyAsMetafile = new PlotDeviceCopyAsMetafileCommand(_interactiveWorkflow, _visualComponent);
            RemoveAllPlots = new PlotDeviceRemoveAllCommand(_interactiveWorkflow, _visualComponent);
            RemoveCurrentPlot = new PlotDeviceRemoveCurrentCommand(_interactiveWorkflow, _visualComponent);
            NextPlot = new PlotDeviceMoveNextCommand(_interactiveWorkflow, _visualComponent);
            PreviousPlot = new PlotDeviceMovePreviousCommand(_interactiveWorkflow, _visualComponent);
            EndLocator = new PlotDeviceEndLocatorCommand(_interactiveWorkflow, _visualComponent);
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
