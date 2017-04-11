// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotDeviceRemoveCurrentCommand : PlotDeviceCommand, IAsyncCommand {
        public PlotDeviceRemoveCurrentCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceVisualComponent visualComponent)
            : base(interactiveWorkflow, visualComponent) {
        }

        public CommandStatus Status {
            get {
                if (HasCurrentPlot && !IsInLocatorMode) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported;
            }
        }

        public async Task InvokeAsync() {
            var msg = string.Format(CultureInfo.CurrentUICulture, Resources.Plots_RemoveCurrentPlotWarning, VisualComponent.DeviceName);
            if (InteractiveWorkflow.Shell.ShowMessage(msg, MessageButtons.YesNo) == MessageButtons.Yes) {
                try {
                    await InteractiveWorkflow.Plots.RemovePlotAsync(VisualComponent.ActivePlot);
                } catch (RPlotManagerException ex) {
                    InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
                } catch (OperationCanceledException) {
                }
            }
        }
    }
}
