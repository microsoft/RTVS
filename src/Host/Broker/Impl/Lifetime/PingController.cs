// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Mvc;

namespace Microsoft.R.Host.Broker.Lifetime {
    [Route("/ping")]
    public class PingController : Controller {
        private readonly LifetimeManager _lifetimeManager;

        public PingController(LifetimeManager lifetimeManager) {
            _lifetimeManager = lifetimeManager;
        }

        [HttpPost]
        public void Post() {
            _lifetimeManager.Ping();
        }
    }
}
