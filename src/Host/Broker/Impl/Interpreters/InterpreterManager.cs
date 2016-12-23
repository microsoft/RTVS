// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Microsoft.Common.Core.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.R.Interpreters;
using static System.FormattableString;

namespace Microsoft.R.Host.Broker.Interpreters {
    public class InterpreterManager {
        private const string _localId = "local";

        private readonly ROptions _options;
        private readonly ILogger _logger;
        private IFileSystem _fs;
        private int _intepretedId = 1;

        public IReadOnlyCollection<Interpreter> Interpreters { get; private set; }

        [ImportingConstructor]
        public InterpreterManager(IOptions<ROptions> options, ILogger<InterpreterManager> logger, IFileSystem fs) {
            _options = options.Value;
            _logger = logger;
            _fs = fs;
        }

        public void Initialize() {
            Interpreters = GetInterpreters().ToArray();

            var sb = new StringBuilder(Invariant($"{Interpreters.Count} interpreters configured:"));
            foreach (var interp in Interpreters) {
                sb.Append(Environment.NewLine + Invariant($"'{interp.Id}': {interp.Version} at \"{interp.Path}\""));
            }
            _logger.LogInformation(sb.ToString());
        }

        private IEnumerable<Interpreter> GetInterpreters() {
            if (_options.AutoDetect) {
                _logger.LogTrace(Resources.Trace_AutoDetectingR);

                var engines = new RInstallation().GetCompatibleEngines();
                if (engines.Any()) {
                    foreach (var e in engines) {
                        var detected = new Interpreter(this, Invariant($"{_intepretedId}"), e.Name, e.InstallPath, e.BinPath, e.Version);
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
                    var interpInfo = new RInterpreterInfo(string.Empty, options.BasePath);
                    if (interpInfo.VerifyInstallation()) {
                        yield return new Interpreter(this, id, options.Name, interpInfo.InstallPath, interpInfo.BinPath, interpInfo.Version);
                        continue;
                    }
                }

                _logger.LogError(Resources.Error_FailedRInstallationData, options.Name ?? id, options.BasePath);
            }
        }
    }
}

