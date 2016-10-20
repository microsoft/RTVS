// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using static System.FormattableString;

namespace Microsoft.R.Host.UserProfile {
    class ServiceLoggerProvider : ILoggerProvider {

        private readonly StreamWriter _writer;
        private readonly List<ServiceLogger> _loggers = new List<ServiceLogger>();

        public ServiceLoggerProvider()
                : this(File.CreateText(GetLogFileName())) {
        }

        public ServiceLoggerProvider(StreamWriter writer) {
            _writer = writer;
        }

        private static string GetLogFileName() {
            return Path.Combine(Path.GetTempPath(), Invariant($@"Microsoft.R.Host.UserProfile_{DateTime.Now:yyyyMdd_HHmmss}_pid{Process.GetCurrentProcess().Id}.log"));
        }

        public ILogger CreateLogger(string categoryName) {
            var logger = new ServiceLogger(categoryName, _writer);
            _loggers.Add(logger);
            return logger;
        }

        public void Dispose() {
            foreach (var logger in _loggers) {
                logger.Dispose();
            }
            _writer.Dispose();
        }
    }
}
