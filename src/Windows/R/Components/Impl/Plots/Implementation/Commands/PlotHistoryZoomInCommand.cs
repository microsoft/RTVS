// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotHistoryZoomInCommand : PlotHistoryCommand, IAsyncCommand {
        public PlotHistoryZoomInCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotHistoryVisualComponent visualComponent) :
            base(interactiveWorkflow, visualComponent) {
        }

        public override CommandStatus Status 
            => VisualComponent.CanIncreaseThumbnailSize 
                ? CommandStatus.SupportedAndEnabled 
                : CommandStatus.Supported;

        public override Task InvokeAsync() {
            VisualComponent.IncreaseThumbnailSize();
            return Task.CompletedTask;
        }
    }
}
