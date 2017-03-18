// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotHistoryCutCopyCommand : PlotHistoryCommand, IAsyncCommand {
        private readonly bool _cut;

        public PlotHistoryCutCopyCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotHistoryVisualComponent visualComponent, bool cut) :
            base(interactiveWorkflow, visualComponent) {
            _cut = cut;
        }

        public CommandStatus Status {
            get {
                var selection = VisualComponent.SelectedPlot;
                if (selection != null && !selection.ParentDevice.LocatorMode) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported;
            }
        }

        public Task InvokeAsync() {
            var selection = VisualComponent.SelectedPlot;
            if (selection != null) {
                try {
                    var data = PlotClipboardData.Serialize(new PlotClipboardData(selection.ParentDevice.DeviceId, selection.PlotId, _cut));
                    Clipboard.Clear();
                    Clipboard.SetData(PlotClipboardData.Format, data);
                } catch (ExternalException ex) {
                    InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
                }
            }
            return Task.CompletedTask;
        }
    }
}
