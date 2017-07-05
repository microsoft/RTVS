// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotHistoryActivateCommand : PlotHistorySelectionCommand, IAsyncCommand {
        public PlotHistoryActivateCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotHistoryVisualComponent visualComponent) :
            base(interactiveWorkflow, visualComponent) {
        }

        protected override Task InvokeAsync(IRPlot plot) => InteractiveWorkflow.Plots.ActivatePlotAsync(plot);
    }
}
