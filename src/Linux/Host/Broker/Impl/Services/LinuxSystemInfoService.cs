// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.R.Host.Broker.Services {
    class LinuxSystemInfoService : ISystemInfoService {
        public double GetCpuLoad() {
            throw new NotImplementedException();
        }

        public (long TotalVirtualMemory, long FreeVirtualMemory, long TotalPhysicalMemory, long FreePhysicalMemory) GetMemoryInformation() {
            throw new NotImplementedException();
        }

        public double GetNetworkLoad() {
            throw new NotImplementedException();
        }

        public (string VideoCardName, long VideoRAM, string VideoProcessor) GetVideoControllerInformation() {
            throw new NotImplementedException();
        }
    }
}