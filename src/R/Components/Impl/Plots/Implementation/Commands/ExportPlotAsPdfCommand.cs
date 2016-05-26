// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class ExportPlotAsPdfCommand : PlotCommand, IAsyncCommand {
        public ExportPlotAsPdfCommand(IRInteractiveWorkflow interactiveWorkflow) : base(interactiveWorkflow) {
        }

        public CommandStatus Status {
            get {
                if (HasCurrentPlot && !IsInLocatorMode) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported;
            }
        }

        public async Task<CommandResult> InvokeAsync() {
            string filePath = InteractiveWorkflow.Shell.ShowSaveFileDialog(Resources.Plots_ExportAsPdfFilter, null, Resources.Plots_ExportAsPdfDialogTitle);
            if (!string.IsNullOrEmpty(filePath)) {
                try {
                    await InteractiveWorkflow.Plots.ExportToPdfAsync(filePath);
                } catch (RPlotManagerException ex) {
                    InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
                } catch (OperationCanceledException) {
                }
            }

            return CommandResult.Executed;
        }
    }
}
