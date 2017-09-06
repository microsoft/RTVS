// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Common.Core.Output;
using Microsoft.VisualStudio.R.Package.Logging;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal sealed class VsOutputService : IOutputService {
        private readonly IServiceContainer _services;
        private readonly Dictionary<string, IOutput> _outputs;
        private IMainThread _mainThread;

        public VsOutputService(IServiceContainer services) {
            _services = services;
            _mainThread = services.MainThread();
            _outputs = new Dictionary<string, IOutput>();
        }

        public async Task<IOutput> GetAsync(string name, CancellationToken cancellationToken) {
            await _mainThread.SwitchToAsync(cancellationToken);
            if (_outputs.TryGetValue(name, out IOutput value)) {
                return value;
            }

            var guid = name.ToGuid();
            var logWriter = new OutputWindowLogWriter(_services, guid, name);
            var output = new LogWriterOutput(logWriter);
            _outputs[name] = output;
            return output;  
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