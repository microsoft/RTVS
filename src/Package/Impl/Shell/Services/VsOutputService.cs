// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.R.Common.Core.Output;
using Microsoft.VisualStudio.R.Package.Logging;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal sealed class VsOutputService : IOutputService {
        private readonly IServiceContainer _services;
        private readonly ConcurrentDictionary<string, IOutput> _outputs;

        public VsOutputService(IServiceContainer services) {
            _services = services;
            _outputs = new ConcurrentDictionary<string, IOutput>();
        }

        public IOutput Get(string name) => _outputs.GetOrAdd(name, CreateOutput);

        private IOutput CreateOutput(string name) {
            var guid = name.ToGuid();
            var logWriter = new OutputWindowLogWriter(_services, guid, name);
            return new LogWriterOutput(logWriter);
        }

        private class LogWriterOutput : IOutput {
            private readonly IActionLogWriter _logWriter;

            public LogWriterOutput(IActionLogWriter logWriter) {
                _logWriter = logWriter;
            }

            public void Write(string text) {
                _logWriter.Write(MessageCategory.General, text);
            }
        }
    }
}