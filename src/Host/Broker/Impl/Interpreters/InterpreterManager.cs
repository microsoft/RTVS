// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Host.Broker.Interpreters {
    public class InterpreterManager {
        private readonly ROptions _options;
        private readonly ILogger _logger;
        private readonly RInstallation _rInstallation = new RInstallation();

        public IReadOnlyCollection<Interpreter> Interpreters { get; private set; }

        [ImportingConstructor]
        public InterpreterManager(IOptions<ROptions> options, ILogger<InterpreterManager> logger) {
            _options = options.Value;
            _logger = logger;
        }

        public void Initialize() {
            Interpreters = GetInterpreters().ToArray();
            var sb = new StringBuilder($"{Interpreters.Count} interpreters configured:");
            foreach (var interp in Interpreters) {
                sb.Append(Environment.NewLine + $"'{interp.Id}': {interp.Version} at \"{interp.Path}\"");
            }
            _logger.LogInformation(sb.ToString());
        }

        private IEnumerable<Interpreter> GetInterpreters() {
            if (_options.AutoDetect) {
                _logger.LogTrace("Auto-detecting R ...");

                var rid = _rInstallation.GetInstallationData(null, new SupportedRVersionRange());
                if (rid.Status == RInstallStatus.OK) {
                    var detected = new Interpreter(this, "", rid.Path, rid.BinPath, rid.Version);
                    _logger.LogTrace($"R {detected.Version} detected at \"{detected.Path}\".");
                    yield return detected;
                } else {
                    _logger.LogWarning("No R interpreters found.");
                }
            }

            foreach (var kv in _options.Interpreters) {
                var rid = _rInstallation.GetInstallationData(kv.Value.BasePath, new SupportedRVersionRange());
                if (rid.Status != RInstallStatus.OK) {
                    _logger.LogError($"Failed to retrieve R installation data for \"{kv.Value.BasePath}\" ({rid.Status})");
                    continue;
                }

                yield return new Interpreter(this, kv.Key, rid.Path, rid.BinPath, rid.Version);
            }
        }
    }
}

