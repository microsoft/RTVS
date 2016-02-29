// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal class PlotWindowCommand : PackageCommand {
        protected IPlotHistory PlotHistory { get; }

        public PlotWindowCommand(IPlotHistory plotHistory, int id) : 
            base(RGuidList.RCmdSetGuid, id) {
            PlotHistory = plotHistory;
        }
    }
}
