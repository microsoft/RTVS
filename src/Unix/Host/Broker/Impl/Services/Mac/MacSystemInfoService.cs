// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.Services.Mac {
    internal sealed class MacSystemInfoService : ISystemInfoService {
        public double GetCpuLoad() => 0;
        public (long TotalVirtualMemory, long FreeVirtualMemory, long TotalPhysicalMemory, long FreePhysicalMemory) GetMemoryInformation()
            => (16, 12, 16, 12);
        public double GetNetworkLoad() => 0;
        public IEnumerable<VideoCardInfo> GetVideoControllerInformation()
            => new[] { new VideoCardInfo() {
                VideoCardName = "NVIDIA",
                VideoProcessor = "NVIDIA",
                VideoRAM = 512
        } };
    }
}
