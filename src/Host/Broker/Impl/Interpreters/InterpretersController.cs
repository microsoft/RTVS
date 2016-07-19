// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.R.Interpreters;
using Microsoft.Extensions.Options;
using Microsoft.R.Host.Broker.Security;

namespace Microsoft.R.Host.Broker.Interpreters {
    [Authorize(Policy = Policies.RUser)]
    [Route("/interpreters")]
    public class InterpretersController : Controller {
        private readonly IOptions<InterpretersOptions> _interpOptions;
        private readonly RInstallation rInstallation = new RInstallation();

        public InterpretersController(IOptions<InterpretersOptions> interpOptions) {
            _interpOptions = interpOptions;
        }

        [HttpGet]
        public IEnumerable<InterpreterInfo> Get() {
            if (_interpOptions.Value.AutoDetect) {
                var detectedInfo = GetInterpreterInfo("", null, throwOnError: false);
                if (detectedInfo != null) {
                    yield return detectedInfo;
                }
            }

            foreach (var kv in _interpOptions.Value.Interpreters) {
                yield return GetInterpreterInfo(kv.Key, kv.Value.BasePath, throwOnError: true);
            }
        }

        private InterpreterInfo GetInterpreterInfo(string name, string basePath, bool throwOnError) {
            var rid = rInstallation.GetInstallationData(basePath, new SupportedRVersionRange());
            if (rid.Status != RInstallStatus.OK) {
                if (throwOnError) {
                    throw rid.Exception ?? new InvalidOperationException("Failed to retrieve R installation data");
                } else {
                    return null;
                }
            }

            return new InterpreterInfo {
                Name = name,
                Path = rid.Path,
                BinPath = rid.BinPath,
                Version = rid.Version,
            };
        }
    }
}
