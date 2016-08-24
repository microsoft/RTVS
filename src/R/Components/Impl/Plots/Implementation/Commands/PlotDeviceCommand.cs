// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots.ViewModel;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal abstract class PlotDeviceCommand : InteractiveWorkflowAsyncCommand {
        protected IRPlotDeviceViewModel ViewModel { get; }

        public PlotDeviceCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotDeviceViewModel viewModel) :
            base(interactiveWorkflow) {
            if (viewModel == null) {
                throw new ArgumentNullException(nameof(viewModel));
            }

            ViewModel = viewModel;
        }

        protected bool IsInLocatorMode {
            get {
                return ViewModel.LocatorMode;
            }
        }

        protected bool HasCurrentPlot {
            get {
                return ViewModel.HasPlot;
            }
        }
    }
}
