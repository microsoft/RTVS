// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Components.Plots;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;

namespace Microsoft.VisualStudio.R.Package.Plots {
    [Export(typeof(IPlotHistoryProvider))]
    class PlotHistoryProvider : IPlotHistoryProvider {
        private PlotHistory _instance;
        public IPlotHistory GetPlotHistory(IRSession session) {
            if(_instance == null) {
                _instance = new PlotHistory(session);
            }
            return _instance;
        }
    }
}
