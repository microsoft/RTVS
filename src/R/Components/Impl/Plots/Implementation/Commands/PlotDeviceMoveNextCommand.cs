// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots.ViewModel;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotDeviceMoveNextCommand : PlotDeviceCommand, IAsyncCommand {
        public PlotDeviceMoveNextCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceViewModel viewModel) :
            base(interactiveWorkflow, viewModel) {
        }

        public CommandStatus Status {
            get {
                if (ViewModel.ActivePlotIndex >= 0 &&
                    ViewModel.ActivePlotIndex < ViewModel.PlotCount - 1 &&
                    !IsInLocatorMode) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported;
            }
        }

        public async Task<CommandResult> InvokeAsync() {
            try {
                await ViewModel.NextPlotAsync();
            } catch (RPlotManagerException ex) {
                InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
            } catch (OperationCanceledException) {
            }

            return CommandResult.Executed;
        }
    }
}
