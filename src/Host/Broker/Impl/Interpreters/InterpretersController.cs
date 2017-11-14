// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.Interpreters {
    [Authorize(Policy = Policies.RUser)]
    [Route("/interpreters")]
    public class InterpretersController : Controller {
        private readonly InterpreterManager _interpManager;

        public InterpretersController(InterpreterManager interpManager) {
            _interpManager = interpManager;
        }

        [HttpGet]
        public IEnumerable<InterpreterInfo> Get() {
            return _interpManager.Interpreters.Select(x => new InterpreterInfo {
                Id = x.Id,
                Name = x.Name,
                Version = x.Version,
                Path = x.InstallPath,
                BinPath = x.BinPath,
                LibPath = x.LibPath,
            });
        }
    }
}
