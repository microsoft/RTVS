// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Broker.Services;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.About {
    //[Authorize(Policy = Policies.RUser)]
    [Route("/info/load")]
    public class LoadController : Controller {
        private readonly Timer _timer;
        private readonly HostLoad _hostLoad = new HostLoad();
        private readonly ISystemInfoService _systemInfo;

        public LoadController(ISystemInfoService systemInfo) {
            _systemInfo = systemInfo;
            _timer = new Timer(UpdateMeasurement, null, 0, 3000);
            UpdateMeasurement(null);
        }

        [AllowAnonymous]
        [HttpGet]
        public HostLoad Get() {
            return _hostLoad;
        }

        private void UpdateMeasurement(object state) {
            var memoryInfo = _systemInfo.GetMemoryInformation();
            _hostLoad.MemoryLoad = (double)(memoryInfo.TotalPhysicalMemory - memoryInfo.FreePhysicalMemory) / memoryInfo.TotalPhysicalMemory;
            _hostLoad.CpuLoad = _systemInfo.GetCpuLoad();
            _hostLoad.NetworkLoad = _systemInfo.GetNetworkLoad();
        }

        protected override void Dispose(bool disposing) {
            _timer?.Dispose();
            base.Dispose(disposing);
        }
    }
}