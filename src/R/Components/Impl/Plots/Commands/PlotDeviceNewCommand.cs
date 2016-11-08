// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Commands {
    public sealed class PlotDeviceNewCommand : IAsyncCommand {
        private readonly IRInteractiveWorkflow _workflow;

        public PlotDeviceNewCommand(IRInteractiveWorkflow workflow) {
            if (workflow == null) {
                throw new ArgumentNullException(nameof(workflow));
            }

            _workflow = workflow;
        }

        public CommandStatus Status {
            get {
                return CommandStatus.SupportedAndEnabled;
            }
        }

        public async Task<CommandResult> InvokeAsync() {
            try {
                await _workflow.Plots.NewDeviceAsync(-1);
            } catch (RPlotManagerException ex) {
                _workflow.Shell.ShowErrorMessage(ex.Message);
            } catch (OperationCanceledException) {
            }

            return CommandResult.Executed;
        }
    }
}
