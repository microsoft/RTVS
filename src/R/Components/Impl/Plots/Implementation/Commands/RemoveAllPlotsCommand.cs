// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class RemoveAllPlotsCommand : PlotCommand, IAsyncCommand {
        public RemoveAllPlotsCommand(IRInteractiveWorkflow interactiveWorkflow) : base(interactiveWorkflow) {
        }

        public CommandStatus Status {
            get {
                if (InteractiveWorkflow.Plots.PlotCount > 0 && !IsInLocatorMode) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported;
            }
        }

        public async Task<CommandResult> InvokeAsync() {
            if (InteractiveWorkflow.Shell.ShowMessage(Resources.Plots_RemoveAllPlotsWarning, MessageButtons.YesNo) == MessageButtons.Yes) {
                try {
                    await InteractiveWorkflow.Plots.RemoveAllPlotsAsync();
                } catch (RPlotManagerException ex) {
                    InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
                } catch (OperationCanceledException) {
                }
            }

            return CommandResult.Executed;
        }
    }
}
