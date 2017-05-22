// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.R.Interpreters;
using static System.FormattableString;

namespace Microsoft.R.Host.Broker.Interpreters {
    public class InterpreterManager {
        private readonly ROptions _options;
        private readonly ILogger _logger;
        private readonly IFileSystem _fs;
        private readonly IRInstallationService _installationService;

        public IReadOnlyCollection<Interpreter> Interpreters { get; private set; }

        public InterpreterManager(IFileSystem fs, IRInstallationService installationService, IOptions<ROptions> options, ILogger<InterpreterManager> logger) {
            _options = options.Value;
            _logger = logger;
            _fs = fs;
            _installationService = installationService;
        }

        public void Initialize() {
            Interpreters = GetInterpreters().ToArray();

            var sb = new StringBuilder(Invariant($"{Interpreters.Count} interpreters configured:"));
            foreach (var interp in Interpreters) {
                sb.Append(Environment.NewLine + Invariant($"[{interp.Id}] : {interp.Name} at \"{interp.Path}\""));
            }
            _logger.LogInformation(sb.ToString());
        }

        private IEnumerable<Interpreter> GetInterpreters() {
            if (_options.AutoDetect) {
                _logger.LogTrace(Resources.Trace_AutoDetectingR);

                var engines = _installationService.GetCompatibleEngines().AsList();
                if (engines.Any()) {
                    var interpreterId = 0;
                    foreach (var e in engines) {
                        var detected = new Interpreter(this, Invariant($"{interpreterId++}"), e);
                        _logger.LogTrace(Resources.Trace_DetectedR, detected.Version, detected.Path);
                        yield return detected;
                    }
                } else {
                    _logger.LogWarning(Resources.Error_NoRInterpreters);
                }
            }

            foreach (var kv in _options.Interpreters) {
                string id = kv.Key;
                InterpreterOptions options = kv.Value;

                if (!string.IsNullOrEmpty(options.BasePath) && _fs.DirectoryExists(options.BasePath)) {
                    var interpInfo = _installationService.CreateInfo(string.Empty, options.BasePath);
                    if (interpInfo != null && interpInfo.VerifyInstallation()) {
                        yield return new Interpreter(this, id, options.Name, interpInfo);
                        continue;
                    }
                }

                _logger.LogError(Resources.Error_FailedRInstallationData, options.Name ?? id, options.BasePath);
            }
        }
    }
}

