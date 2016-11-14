// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Protocol {
    public class HostLoad {
        public long FreeVirtualMemory { get; set; }
        public long FreePhysicalMemory { get; set; }
        public float CpuLoad { get; set; }
        public float NetworkLoad { get; set; }
    }
}
