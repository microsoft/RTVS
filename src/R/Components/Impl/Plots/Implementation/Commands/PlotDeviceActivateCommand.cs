// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotDeviceActivateCommand : PlotDeviceCommand, IAsyncCommand {
        public PlotDeviceActivateCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceViewModel viewModel)
            : base(interactiveWorkflow, viewModel) {
        }

        public CommandStatus Status {
            get {
                if (ViewModel.IsDeviceActive) {
                    return CommandStatus.SupportedAndEnabled | CommandStatus.Latched;
                }
                return CommandStatus.SupportedAndEnabled;
            }
        }

        public async Task<CommandResult> InvokeAsync() {
            try {
                await ViewModel.ActivateDeviceAsync();
            } catch (RPlotManagerException ex) {
                InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
            } catch (OperationCanceledException) {
            }

            return CommandResult.Executed;
        }
    }
}
