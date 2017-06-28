// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotDevicePasteCommand : PlotDeviceCommand, IAsyncCommand {
        public PlotDevicePasteCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceVisualComponent visualComponent)
            : base(interactiveWorkflow, visualComponent) {
        }

        public CommandStatus Status {
            get {
                var dataObject = Clipboard.GetDataObject();
                if (dataObject != null && !IsInLocatorMode && dataObject.GetDataPresent(PlotClipboardData.Format)) {
                    return CommandStatus.SupportedAndEnabled;
                }
                return CommandStatus.Supported;
            }
        }

        public async Task InvokeAsync() {
            try {
                if (Clipboard.ContainsData(PlotClipboardData.Format)) {
                    var serialized = Clipboard.GetData(PlotClipboardData.Format) as string[];
                    var sources = (serialized?.Select(PlotClipboardData.Parse) ?? Enumerable.Empty<PlotClipboardData>()).ToArray();
                    if (sources.Length > 0) {
                        try {
                            if (VisualComponent.Device == null) {
                                await InteractiveWorkflow.Plots.NewDeviceAsync(VisualComponent.InstanceId);
                            }

                            Debug.Assert(VisualComponent.Device != null);
                            foreach (var source in sources) {
                                await InteractiveWorkflow.Plots.CopyOrMovePlotFromAsync(source.DeviceId, source.PlotId,
                                    VisualComponent.Device, source.Cut);
                            }

                            // If it's a move, clear the clipboard as we don't want
                            // the user to try to paste it again
                            if (sources[0].Cut) {
                                Clipboard.Clear();
                            }
                        } catch (RPlotManagerException ex) {
                            InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
                        } catch (OperationCanceledException) {
                        }
                    }
                }
            } catch (ExternalException ex) {
                InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
            }
        }
    }
}
