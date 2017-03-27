// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI.Commands;

namespace Microsoft.R.Components.ConnectionManager {
    public static class ConnectionManagerCommandIds {
        private const string RCmdSetGuidString = "AD87578C-B324-44DC-A12A-B01A6ED5C6E3";
        private static readonly Guid RCmdSetGuid = new Guid(RCmdSetGuidString);

        public static CommandId ContextMenu { get; } = new CommandId(RCmdSetGuid, 105);
    }
}
