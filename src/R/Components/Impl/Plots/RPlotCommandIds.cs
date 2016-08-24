// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Design;

namespace Microsoft.R.Components.Plots {
    public static class RPlotCommandIds {
        private const string RCmdSetGuidString = "AD87578C-B324-44DC-A12A-B01A6ED5C6E3";
        private static readonly Guid RCmdSetGuid = new Guid(RCmdSetGuidString);

        public static CommandID PlotHistoryContextMenu { get; } = new CommandID(RCmdSetGuid, 106);
        public static CommandID PlotDeviceContextMenu { get; } = new CommandID(RCmdSetGuid, 107);
    }
}
