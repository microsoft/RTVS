// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotHistoryCutCopyCommand : InteractiveWorkflowAsyncCommand, IAsyncCommand {
        private bool _cut;

        public PlotHistoryCutCopyCommand(IRInteractiveWorkflow interactiveWorkflow, bool cut) :
            base(interactiveWorkflow) {
            _cut = cut;
        }

        public CommandStatus Status {
            get {
                var selection = InteractiveWorkflow.Plots.History.SelectedPlot;
                if (selection != null) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported;
            }
        }

        public Task<CommandResult> InvokeAsync() {
            var selection = InteractiveWorkflow.Plots.History.SelectedPlot;
            if (selection != null) {
                try {
                    Clipboard.Clear();
                    Clipboard.SetData(PlotClipboardData.Format, 
                        new PlotClipboardData(selection.DeviceId, selection.PlotId, InteractiveWorkflow.RSession.ProcessId, _cut).ToString());
                } catch (ExternalException ex) {
                    InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
                }
            }
            return Task.FromResult(CommandResult.Executed);
        }
    }
}
