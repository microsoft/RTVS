// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots.Implementation.ViewModel;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotHistoryZoomOutCommand : InteractiveWorkflowAsyncCommand, IAsyncCommand {
        public PlotHistoryZoomOutCommand(IRInteractiveWorkflow interactiveWorkflow) :
            base(interactiveWorkflow) {
        }

        public CommandStatus Status {
            get {
                if (InteractiveWorkflow.Plots.History.ThumbnailSize > RPlotHistoryViewModel.MinThumbnailSize) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported;
            }
        }

        public Task<CommandResult> InvokeAsync() {
            InteractiveWorkflow.Plots.History.DecreaseThumbnailSize();
            return Task.FromResult(CommandResult.Executed);
        }
    }
}
