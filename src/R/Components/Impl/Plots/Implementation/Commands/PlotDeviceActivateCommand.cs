// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotDeviceActivateCommand : PlotDeviceCommand, IAsyncCommand {
        public PlotDeviceActivateCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceVisualComponent visualComponent)
            : base(interactiveWorkflow, visualComponent) {
        }

        public CommandStatus Status {
            get {
                if (VisualComponent.IsDeviceActive) {
                    return CommandStatus.SupportedAndEnabled | CommandStatus.Latched;
                }
                return CommandStatus.SupportedAndEnabled;
            }
        }

        public async Task<CommandResult> InvokeAsync() {
            try {
                if (VisualComponent.Device == null) {
                    await InteractiveWorkflow.Plots.NewDeviceAsync(VisualComponent.InstanceId);
                } else {
                    await InteractiveWorkflow.Plots.ActivateDeviceAsync(VisualComponent.Device);
                }
            } catch (RPlotManagerException ex) {
                InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
            } catch (OperationCanceledException) {
            }

            return CommandResult.Executed;
        }
    }
}
