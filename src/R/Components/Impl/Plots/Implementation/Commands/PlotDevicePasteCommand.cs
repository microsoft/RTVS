// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotDevicePasteCommand : PlotDeviceCommand, IAsyncCommand {
        public PlotDevicePasteCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceVisualComponent visualComponent)
            : base(interactiveWorkflow, visualComponent) {
        }

        public CommandStatus Status {
            get {
                if (!IsInLocatorMode && Clipboard.GetDataObject().GetDataPresent(PlotClipboardData.Format)) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported;
            }
        }

        public async Task<CommandResult> InvokeAsync() {
            if (Clipboard.ContainsData(PlotClipboardData.Format)) {
                var source = PlotClipboardData.Parse((string)Clipboard.GetData(PlotClipboardData.Format));
                if (source != null) {
                    try {
                        if (VisualComponent.Device == null) {
                            await InteractiveWorkflow.Plots.NewDeviceAsync(VisualComponent.InstanceId);
                        }

                        Debug.Assert(VisualComponent.Device != null);
                        await InteractiveWorkflow.Plots.CopyOrMovePlotFromAsync(source.DeviceId, source.PlotId, VisualComponent.Device, source.Cut);

                        // If it's a move, clear the clipboard as we don't want
                        // the user to try to paste it again
                        if (source.Cut) {
                            Clipboard.Clear();
                        }
                    } catch (RPlotManagerException ex) {
                        InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
                    } catch (OperationCanceledException) {
                    }
                }
            }

            return CommandResult.Executed;
        }
    }
}
