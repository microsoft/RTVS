// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class HistoryPreviousPlotCommand : PlotWindowCommand {
        public HistoryPreviousPlotCommand(IPlotHistory plotHistory) :
            base(plotHistory, RPackageCommandId.icmdPrevPlot) {
        }
        protected override void SetStatus() {
            Enabled = PlotHistory.ActivePlotIndex > 0;
        }
        protected override void Handle() {
            PlotContentProvider.DoNotWait(PlotHistory.PlotContentProvider.PreviousPlotAsync());
        }
    }
}
