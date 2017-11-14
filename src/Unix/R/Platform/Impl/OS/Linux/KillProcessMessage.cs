// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core.OS.Linux {
    internal class KillProcessMessage {
        public string Name { get; } = "KillProcess";
        public int ProcessId { get; set; }
    }
}
