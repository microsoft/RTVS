// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.Services {
    public interface ISystemInfoService {
        double GetCpuLoad();
        double GetNetworkLoad();
        (long TotalVirtualMemory, long FreeVirtualMemory, long TotalPhysicalMemory, long FreePhysicalMemory) GetMemoryInformation();
        IEnumerable<VideoCardInfo> GetVideoControllerInformation();
    }
}