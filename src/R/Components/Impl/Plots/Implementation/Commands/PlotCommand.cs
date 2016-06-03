// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal abstract class PlotCommand {
        protected IRInteractiveWorkflow InteractiveWorkflow { get; }

        public PlotCommand(IRInteractiveWorkflow interactiveWorkflow) {
            InteractiveWorkflow = interactiveWorkflow;
        }

        protected bool IsInLocatorMode => InteractiveWorkflow.Plots.IsInLocatorMode;

        protected bool HasCurrentPlot => InteractiveWorkflow.Plots.ActivePlotIndex >= 0;
    }
}
