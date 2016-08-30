// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotDeviceMovePreviousCommand : PlotDeviceCommand, IAsyncCommand {
        public PlotDeviceMovePreviousCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceVisualComponent visualComponent) :
            base(interactiveWorkflow, visualComponent) {
        }

        public CommandStatus Status {
            get {
                if (VisualComponent.ActivePlotIndex > 0 &&
                    !IsInLocatorMode) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported;
            }
        }

        public async Task<CommandResult> InvokeAsync() {
            try {
                await VisualComponent.PreviousPlotAsync();
            } catch (RPlotManagerException ex) {
                InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
            } catch (OperationCanceledException) {
            }

            return CommandResult.Executed;
        }
    }
}
