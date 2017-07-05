// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotHistoryRemoveCommand : PlotHistorySelectionCommand, IAsyncCommand {
        public PlotHistoryRemoveCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotHistoryVisualComponent visualComponent) :
            base(interactiveWorkflow, visualComponent) {
        }

        protected override bool CanInvoke()
            => InteractiveWorkflow.Shell.ShowMessage(Resources.Plots_RemoveSelectedPlotWarning, MessageButtons.YesNo) == MessageButtons.Yes;

        protected override Task InvokeAsync(IRPlot plot) => InteractiveWorkflow.Plots.RemovePlotAsync(plot);
    }
}
