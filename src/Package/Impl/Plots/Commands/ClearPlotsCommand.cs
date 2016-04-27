// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Plots;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class ClearPlotsCommand : PlotWindowCommand {
        public ClearPlotsCommand(IPlotHistory plotHistory) :
            base(plotHistory, RPackageCommandId.icmdClearPlots) {
        }

        protected override void SetStatus() {
            Enabled = PlotHistory.PlotCount > 0 && !IsInLocatorMode;
        }

        protected override void Handle() {
            if (VsAppShell.Current.ShowMessage(Resources.DeleteAllPlots, MessageButtons.YesNo) == MessageButtons.Yes) {
                PlotContentProvider.DoNotWait(PlotHistory.PlotContentProvider.ClearAllAsync());
            }
        }
    }
}
