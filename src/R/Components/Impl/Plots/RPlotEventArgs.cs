// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.Plots {
    public class RPlotEventArgs : EventArgs {
        public IRPlot Plot { get; }

        public RPlotEventArgs(IRPlot plot) {
            Plot = plot;
        }
    }
}
