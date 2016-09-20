// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots.Implementation.Commands;

namespace Microsoft.R.Components.Plots {
    public class RPlotHistoryCommands {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly IRPlotHistoryVisualComponent _visualComponent;

        public RPlotHistoryCommands(IRInteractiveWorkflow interactiveWorkflow, IRPlotHistoryVisualComponent visualComponent) {
            if (interactiveWorkflow == null) {
                throw new ArgumentNullException(nameof(interactiveWorkflow));
            }

            if (visualComponent == null) {
                throw new ArgumentNullException(nameof(visualComponent));
            }

            _interactiveWorkflow = interactiveWorkflow;
            _visualComponent = visualComponent;

            ZoomIn = new PlotHistoryZoomInCommand(_interactiveWorkflow, _visualComponent);
            ZoomOut = new PlotHistoryZoomOutCommand(_interactiveWorkflow, _visualComponent);
            AutoHide = new PlotHistoryAutoHideCommand(_interactiveWorkflow, _visualComponent);
            Cut = new PlotHistoryCutCopyCommand(_interactiveWorkflow, _visualComponent, cut: true);
            Copy = new PlotHistoryCutCopyCommand(_interactiveWorkflow, _visualComponent, cut: false);
            Remove = new PlotHistoryRemoveCommand(_interactiveWorkflow, _visualComponent);
            ActivatePlot = new PlotHistoryActivateCommand(_interactiveWorkflow, _visualComponent);
        }

        public IAsyncCommand ZoomIn { get; }
        public IAsyncCommand ZoomOut { get; }
        public IAsyncCommand AutoHide { get; }
        public IAsyncCommand Cut { get; }
        public IAsyncCommand Copy { get; }
        public IAsyncCommand Remove { get; }
        public IAsyncCommand ActivatePlot { get; }
    }
}
