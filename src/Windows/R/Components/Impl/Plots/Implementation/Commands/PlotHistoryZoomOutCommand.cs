// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotHistoryZoomOutCommand : PlotHistoryCommand, IAsyncCommand {
        public PlotHistoryZoomOutCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotHistoryVisualComponent visualComponent) :
            base(interactiveWorkflow, visualComponent) {
        }

        public override CommandStatus Status 
            => VisualComponent.CanDecreaseThumbnailSize 
                ? CommandStatus.SupportedAndEnabled 
                : CommandStatus.Supported;

        public override Task InvokeAsync() {
            VisualComponent.DecreaseThumbnailSize();
            return Task.CompletedTask;
        }
    }
}
