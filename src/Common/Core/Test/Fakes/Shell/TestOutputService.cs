// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.R.Common.Core.Output;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    public class TestOutputService : IOutputService {
        private readonly IServiceContainer _services;
        private readonly ConcurrentDictionary<string, IOutput> _outputs;

        public TestOutputService(IServiceContainer services) {
            _services = services;
            _outputs = new ConcurrentDictionary<string, IOutput>();
        }

        public IOutput Get(string name) 
            => _outputs.GetOrAdd(name, prefix => new TestOutput(prefix, _services.Log()));

        private class TestOutput : IOutput {
            private readonly string _prefix;
            private readonly IActionLog _log;

            public TestOutput(string prefix, IActionLog log) {
                _prefix = prefix;
                _log = log;
            }

            public void Write(string text) {
                _log.Write(LogVerbosity.Minimal, MessageCategory.General, $"[{_prefix} output]: {text}");
            }

            public void WriteError(string text) {
                _log.Write(LogVerbosity.Minimal, MessageCategory.Error, $"[{_prefix} output]: {text}");
            }
        }
    }
}