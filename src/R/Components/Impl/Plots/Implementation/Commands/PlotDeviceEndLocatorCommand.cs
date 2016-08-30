// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotDeviceEndLocatorCommand : PlotDeviceCommand, IAsyncCommand {
        public PlotDeviceEndLocatorCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceVisualComponent visualComponent)
            : base(interactiveWorkflow, visualComponent) {
        }

        public CommandStatus Status {
            get {
                if (IsInLocatorMode) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported | CommandStatus.Invisible;
            }
        }

        public Task<CommandResult> InvokeAsync() {
            VisualComponent.EndLocatorMode();
            return Task.FromResult(CommandResult.Executed);
        }
    }
}
