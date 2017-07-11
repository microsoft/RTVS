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

        public CommandStatus Status
            => HasCurrentPlot && !IsInLocatorMode
                ? CommandStatus.SupportedAndEnabled
                : CommandStatus.Supported;

        public Task InvokeAsync() {
            try {
                PlotClipboardData.ToClipboard(VisualComponent.Device.DeviceId, VisualComponent.Device.ActivePlot.PlotId, _cut);
            } catch (ExternalException ex) {
                InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
            }
            return Task.CompletedTask;
        }
    }
}
