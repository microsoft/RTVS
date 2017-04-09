// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotDeviceCutCopyCommand : PlotDeviceCommand, IAsyncCommand {
        private readonly bool _cut;

        public PlotDeviceCutCopyCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceVisualComponent visualComponent, bool cut)
            : base(interactiveWorkflow, visualComponent) {
            _cut = cut;
        }

        public CommandStatus Status {
            get {
                if (HasCurrentPlot && !IsInLocatorMode) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported;
            }
        }

        public Task InvokeAsync() {
            try {
                var data = PlotClipboardData.Serialize(new PlotClipboardData(VisualComponent.Device.DeviceId, VisualComponent.Device.ActivePlot.PlotId, _cut));
                Clipboard.Clear();
                Clipboard.SetData(PlotClipboardData.Format, data);
            } catch (ExternalException ex) {
                InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
            }

            return Task.CompletedTask;
        }
    }
}
