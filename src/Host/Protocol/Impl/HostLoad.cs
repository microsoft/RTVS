// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Protocol {
    public class HostLoad {
        public double CpuLoad { get; set; }
        public double MemoryLoad { get; set; }
        public double NetworkLoad { get; set; }
    }
}
