// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal abstract class PlotHistorySelectionCommand: PlotHistoryCommand {
        protected PlotHistorySelectionCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotHistoryVisualComponent visualComponent) 
            : base(interactiveWorkflow, visualComponent) { }

        public override CommandStatus Status {
            get {
                var selection = VisualComponent.SelectedPlots.ToArray();
                if (selection.Length > 0 && selection.All(p => !p.ParentDevice.LocatorMode)) {
                    return CommandStatus.SupportedAndEnabled;
                }
                return CommandStatus.Supported;
            }
        }
    }
}
