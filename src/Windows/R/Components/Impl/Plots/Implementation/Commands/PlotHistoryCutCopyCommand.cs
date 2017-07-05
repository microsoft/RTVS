// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotHistoryCutCopyCommand : PlotHistorySelectionCommand, IAsyncCommand {
        private readonly bool _cut;

        public PlotHistoryCutCopyCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotHistoryVisualComponent visualComponent, bool cut) :
            base(interactiveWorkflow, visualComponent) {
            _cut = cut;
        }

        public override Task InvokeAsync() {
            var selection = VisualComponent.SelectedPlots.ToList();
            if (selection.Count > 0) {
                try {
                    var data = selection.Select(p => PlotClipboardData.Serialize(new PlotClipboardData(p.ParentDevice.DeviceId, p.PlotId, _cut))).ToArray();
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
