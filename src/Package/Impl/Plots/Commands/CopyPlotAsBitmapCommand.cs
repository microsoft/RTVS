// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.Plots;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class CopyPlotAsBitmapCommand : PlotWindowCommand {
        public CopyPlotAsBitmapCommand(IPlotHistory plotHistory) :
            base(plotHistory, RPackageCommandId.icmdCopyPlotAsBitmap) {
        }

        protected override void SetStatus() {
            Enabled = PlotHistory.ActivePlotIndex >= 0 && !IsInLocatorMode;
        }

        protected override void Handle() {
            PlotHistory.PlotContentProvider.CopyToClipboardAsBitmap();
        }
    }
}
