// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotHistoryActivateCommand : InteractiveWorkflowAsyncCommand, IAsyncCommand {
        public PlotHistoryActivateCommand(IRInteractiveWorkflow interactiveWorkflow) :
            base(interactiveWorkflow) {
        }

        public CommandStatus Status {
            get {
                var selection = InteractiveWorkflow.Plots.History.SelectedPlot;
                if (selection != null) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported;
            }
        }

        public async Task<CommandResult> InvokeAsync() {
            var selection = InteractiveWorkflow.Plots.History.SelectedPlot;
            if (selection != null) {
                try {
                    await selection.ActivatePlotAsync();
                } catch (RPlotManagerException ex) {
                    InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
                } catch (OperationCanceledException) {
                }
            }

            return CommandResult.Executed;
        }
    }
}
