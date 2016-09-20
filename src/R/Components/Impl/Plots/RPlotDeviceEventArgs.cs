// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.Plots {
    public class RPlotDeviceEventArgs : EventArgs {
        public IRPlotDevice Device { get; }

        public RPlotDeviceEventArgs(IRPlotDevice device) {
            Device = device;
        }
    }
}
