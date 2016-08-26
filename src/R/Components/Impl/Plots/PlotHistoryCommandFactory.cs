// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots.Implementation.Commands;

namespace Microsoft.R.Components.Plots {
    public static class PlotHistoryCommandFactory {
        public static IAsyncCommand ZoomIn(IRInteractiveWorkflow interactiveWorkflow) {
            return new PlotHistoryZoomInCommand(interactiveWorkflow);
        }

        public static IAsyncCommand ZoomOut(IRInteractiveWorkflow interactiveWorkflow) {
            return new PlotHistoryZoomOutCommand(interactiveWorkflow);
        }

        public static IAsyncCommand AutoHide(IRInteractiveWorkflow interactiveWorkflow) {
            return new PlotHistoryAutoHideCommand(interactiveWorkflow);
        }

        public static IAsyncCommand CutCopy(IRInteractiveWorkflow interactiveWorkflow, bool cut) {
            return new PlotHistoryCutCopyCommand(interactiveWorkflow, cut);
        }

        public static IAsyncCommand Remove(IRInteractiveWorkflow interactiveWorkflow) {
            return new PlotHistoryRemoveCommand(interactiveWorkflow);
        }

        public static IAsyncCommand ActivatePlot(IRInteractiveWorkflow interactiveWorkflow) {
            return new PlotHistoryActivateCommand(interactiveWorkflow);
        }
    }
}
