// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.Plots;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class EndLocatorCommand : PlotWindowCommand {
        public EndLocatorCommand(IPlotHistory plotHistory) :
            base(plotHistory, RPackageCommandId.icmdEndLocator) {
        }

        protected override void SetStatus() {
            Enabled = IsInLocatorMode;
            Visible = Enabled;
        }

        protected override void Handle() {
            if (PlotHistory.PlotContentProvider.Locator != null) {
                PlotHistory.PlotContentProvider.Locator.EndLocatorMode();
            }
        }
    }
}
