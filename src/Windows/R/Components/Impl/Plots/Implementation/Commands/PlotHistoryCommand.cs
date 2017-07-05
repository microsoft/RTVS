// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal abstract class PlotHistoryCommand : InteractiveWorkflowAsyncCommand {
        protected IRPlotHistoryVisualComponent VisualComponent { get; }

        protected PlotHistoryCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotHistoryVisualComponent visualComponent) :
            base(interactiveWorkflow) {
            Check.ArgumentNull(nameof(visualComponent), visualComponent);
            VisualComponent = visualComponent;
        }

        public virtual CommandStatus Status => CommandStatus.NotSupported;

        protected virtual bool CanInvoke() => true;

        protected virtual Task InvokeAsync(IRPlot plot) => Task.CompletedTask;

        public virtual async Task InvokeAsync() {
            var selection = VisualComponent.SelectedPlots.ToArray();
            if (selection.Length > 0 && CanInvoke()) {
                try {
                    foreach (var plot in selection) {
                        await InvokeAsync(plot);
                    }
                } catch (RPlotManagerException ex) {
                    InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
                } catch (OperationCanceledException) {
                }
            }
        }

    }
}
