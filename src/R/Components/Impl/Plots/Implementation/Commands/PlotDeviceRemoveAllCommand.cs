// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots.ViewModel;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotDeviceRemoveAllCommand : PlotDeviceCommand, IAsyncCommand {
        public PlotDeviceRemoveAllCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceViewModel viewModel)
            : base(interactiveWorkflow, viewModel) {
        }

        public CommandStatus Status {
            get {
                if (ViewModel.HasPlot && !IsInLocatorMode) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported;
            }
        }

        public async Task<CommandResult> InvokeAsync() {
            if (InteractiveWorkflow.Shell.ShowMessage(Resources.Plots_RemoveAllPlotsWarning, MessageButtons.YesNo) == MessageButtons.Yes) {
                try {
                    await ViewModel.ClearAllPlotsAsync();
                } catch (RPlotManagerException ex) {
                    InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
                } catch (OperationCanceledException) {
                }
            }

            return CommandResult.Executed;
        }
    }
}
