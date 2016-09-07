// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotHistoryRemoveCommand : PlotHistoryCommand, IAsyncCommand {
        public PlotHistoryRemoveCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotHistoryVisualComponent visualComponent) :
            base(interactiveWorkflow, visualComponent) {
        }

        public CommandStatus Status {
            get {
                var selection = VisualComponent.SelectedPlot;
                if (selection != null && !selection.ParentDevice.LocatorMode) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported;
            }
        }

        public async Task<CommandResult> InvokeAsync() {
            var selection = VisualComponent.SelectedPlot;
            if (selection != null) {
                if (InteractiveWorkflow.Shell.ShowMessage(Resources.Plots_RemoveSelectedPlotWarning, MessageButtons.YesNo) == MessageButtons.Yes) {
                    try {
                        await InteractiveWorkflow.Plots.RemovePlotAsync(selection);
                    } catch (RPlotManagerException ex) {
                        InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
                    } catch (OperationCanceledException) {
                    }
                }
            }

            return CommandResult.Executed;
        }
    }
}
