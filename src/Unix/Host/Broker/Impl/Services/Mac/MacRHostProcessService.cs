// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Interpreters;
using Microsoft.R.Host.Broker.Sessions;
using Microsoft.R.Platform.Host;

namespace Microsoft.R.Host.Broker.Services.Mac {
    internal sealed class MacRHostProcessService : UnixRHostProcessService {
        private readonly IFileSystem _fs;
        public MacRHostProcessService(ILogger<Session> sessionLogger, IFileSystem fs, IProcessServices ps) : base(sessionLogger, ps) {
            _fs = fs;
        }

        protected override string GetRHostBinaryPath() {
            var locator = BrokerExecutableLocator.Create(_fs);
            return locator.GetHostExecutablePath();
        }

        protected override string GetLoadLibraryPath(Interpreter interpreter) {
            var value = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH");
            return !string.IsNullOrEmpty(value) ? value : Path.Combine(interpreter.RInterpreterInfo.InstallPath, "lib");
        }
    }
}