// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Common.Core;
using Microsoft.Common.Wpf.Imaging;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots.ViewModel;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotDeviceCutCopyCommand : PlotDeviceCommand, IAsyncCommand {
        private bool _cut;

        public PlotDeviceCutCopyCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceViewModel viewModel, bool cut)
            : base(interactiveWorkflow, viewModel) {
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

        public Task<CommandResult> InvokeAsync() {
            try {
                Clipboard.Clear();
                Clipboard.SetData(PlotClipboardData.Format,
                    new PlotClipboardData(ViewModel.DeviceId, ViewModel.ActivePlotId, InteractiveWorkflow.RSession.ProcessId, _cut).ToString());
            } catch (ExternalException ex) {
                InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
            }

            return Task.FromResult(CommandResult.Executed);
        }
    }
}
