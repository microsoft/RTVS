// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal abstract class PlotDeviceCommand : InteractiveWorkflowAsyncCommand {
        protected IRPlotDeviceVisualComponent VisualComponent { get; }

        protected PlotDeviceCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceVisualComponent visualComponent) :
            base(interactiveWorkflow) {
            Check.ArgumentNull(nameof(visualComponent), visualComponent);
            VisualComponent = visualComponent;
        }

        protected bool IsInLocatorMode {
            get {
                return VisualComponent.Device?.LocatorMode ?? false;
            }
        }

        protected bool HasCurrentPlot {
            get {
                return VisualComponent.HasPlot;
            }
        }
    }
}
