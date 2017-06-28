// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotHistoryAutoHideCommand : PlotHistoryCommand, IAsyncCommand {
        public PlotHistoryAutoHideCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotHistoryVisualComponent visualComponent) :
            base(interactiveWorkflow, visualComponent) {
        }

        public override CommandStatus Status
            => VisualComponent.AutoHide
                    ? CommandStatus.SupportedAndEnabled | CommandStatus.Latched
                    : CommandStatus.SupportedAndEnabled;

        public override Task InvokeAsync() {
            VisualComponent.AutoHide = !VisualComponent.AutoHide;
            return Task.CompletedTask;
        }
    }
}
