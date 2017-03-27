// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Commands {
    public sealed class PlotDeviceNewCommand : IAsyncCommand {
        private readonly IRInteractiveWorkflow _workflow;

        public PlotDeviceNewCommand(IRInteractiveWorkflow workflow) {
            Check.ArgumentNull(nameof(workflow), workflow);
            _workflow = workflow;
        }

        public CommandStatus Status => CommandStatus.SupportedAndEnabled;

        public async Task InvokeAsync() {
            try {
                await _workflow.Plots.NewDeviceAsync(-1);
            } catch (RPlotManagerException ex) {
                _workflow.Shell.ShowErrorMessage(ex.Message);
            } catch (OperationCanceledException) {
            }
        }
    }
}
