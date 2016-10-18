// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using static System.FormattableString;

namespace Microsoft.R.Host.Broker.Logging {
    internal sealed class FileLoggerProvider : ILoggerProvider {
        private readonly StreamWriter _writer;
        private readonly List<FileLogger> _loggers = new List<FileLogger>();

        public FileLoggerProvider(string name = null)
            : this(File.CreateText(GetLogFileName(name))) {
        }

        public FileLoggerProvider(StreamWriter writer) {
            _writer = writer;
        }

        private static string GetLogFileName(string name) {
            if (!string.IsNullOrEmpty(name)) {
                name = "_" + name;
            }
            return Path.Combine(Path.GetTempPath(), Invariant($@"Microsoft.R.Host.Broker{name}_{DateTime.Now:yyyyMdd_HHmmss}_pid{Process.GetCurrentProcess().Id}.log"));
        }

        public ILogger CreateLogger(string categoryName) {
            var logger = new FileLogger(categoryName, _writer);
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
