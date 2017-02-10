// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Microsoft.Common.Core.Logging {
    public class EventLogLoggerProvider : ILoggerProvider {
        private readonly List<EventLogLogger> _loggers = new List<EventLogLogger>();
        private readonly LogLevel _logLevel;
        private readonly string _logSource;

        public EventLogLoggerProvider(LogLevel minLogLevel, string source) {
            _logLevel = minLogLevel;
            _logSource = source;
        }

        public ILogger CreateLogger(string categoryName) {
            var logger = new EventLogLogger(categoryName, _logLevel, _logSource);
            _loggers.Add(logger);
            return logger;
        }

        public void Dispose() {
            foreach (var logger in _loggers) {
                logger.Dispose();
            }
        }
    }
}
