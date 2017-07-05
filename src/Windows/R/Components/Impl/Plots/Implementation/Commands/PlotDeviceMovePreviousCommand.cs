// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotDeviceMovePreviousCommand : PlotDeviceCommand, IAsyncCommand {
        public PlotDeviceMovePreviousCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceVisualComponent visualComponent) :
            base(interactiveWorkflow, visualComponent) {
        }

        public CommandStatus Status
            => VisualComponent.ActivePlotIndex > 0 && !IsInLocatorMode 
                    ? CommandStatus.SupportedAndEnabled 
                    : CommandStatus.Supported;

        public async Task InvokeAsync() {
            try {
                await InteractiveWorkflow.Plots.PreviousPlotAsync(VisualComponent.Device);
            } catch (RPlotManagerException ex) {
                InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
            } catch (OperationCanceledException) {
            }
        }
    }
}
