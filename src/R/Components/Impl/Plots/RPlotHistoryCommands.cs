// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots.Implementation.Commands;

namespace Microsoft.R.Components.Plots {
    public class RPlotHistoryCommands {
        public RPlotHistoryCommands(IRInteractiveWorkflow interactiveWorkflow, IRPlotHistoryVisualComponent visualComponent) {
            if (interactiveWorkflow == null) {
                throw new ArgumentNullException(nameof(interactiveWorkflow));
            }

            if (visualComponent == null) {
                throw new ArgumentNullException(nameof(visualComponent));
            }

            ZoomIn = new PlotHistoryZoomInCommand(interactiveWorkflow, visualComponent);
            ZoomOut = new PlotHistoryZoomOutCommand(interactiveWorkflow, visualComponent);
            AutoHide = new PlotHistoryAutoHideCommand(interactiveWorkflow, visualComponent);
            Cut = new PlotHistoryCutCopyCommand(interactiveWorkflow, visualComponent, cut: true);
            Copy = new PlotHistoryCutCopyCommand(interactiveWorkflow, visualComponent, cut: false);
            Remove = new PlotHistoryRemoveCommand(interactiveWorkflow, visualComponent);
            ActivatePlot = new PlotHistoryActivateCommand(interactiveWorkflow, visualComponent);
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
