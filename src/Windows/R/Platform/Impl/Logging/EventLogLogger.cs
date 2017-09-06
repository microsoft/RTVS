// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Common.Core.Disposables;
using Microsoft.Extensions.Logging;

namespace Microsoft.Common.Core.Logging {
    public sealed class EventLogLogger : ILogger, IDisposable {
        private readonly string _category;
        private readonly LogLevel _logLevel;
        private readonly string _eventLogSource;

        public EventLogLogger(string category, LogLevel minLogLevel, string eventLogSource) {
            _category = category;
            _logLevel = minLogLevel;
            _eventLogSource = eventLogSource;
        }

        public void Dispose() { }

        public IDisposable BeginScope<TState>(TState state) => Disposable.Empty;
        public bool IsEnabled(LogLevel logLevel) => logLevel >= _logLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
            if(logLevel < _logLevel) {
                return;
            }

            var message = formatter(state, exception);
            using (var eventLog = new EventLog("Application")) {
                var logType = GetLogType(logLevel);
                eventLog.Source = _eventLogSource;
                string logMessage;
                if (exception != null) {
                    logMessage = string.Format(CultureInfo.CurrentCulture, "[{0:u}] <{1}> ({2}):{3}{4}{5}Exception: {6}", DateTime.Now, _category, logLevel.ToString()[0], Environment.NewLine, message, Environment.NewLine, exception);
                } else {
                    logMessage = string.Format(CultureInfo.CurrentCulture, "[{0:u}] <{1}> ({2}):{3}{4}", DateTime.Now, _category, logLevel.ToString()[0], Environment.NewLine, message);
                }
                eventLog.WriteEntry(logMessage, logType);
            }
        }

        private EventLogEntryType GetLogType(LogLevel logLevel) {
            switch (logLevel) {
                case LogLevel.Trace:
                    return EventLogEntryType.Information;
                case LogLevel.Debug:
                    return EventLogEntryType.Information;
                case LogLevel.Information:
                    return EventLogEntryType.Information;
                case LogLevel.Warning:
                    return EventLogEntryType.Warning;
                case LogLevel.Error:
                    return EventLogEntryType.Error;
                case LogLevel.Critical:
                    return EventLogEntryType.Error;
                case LogLevel.None:
                default:
                    return EventLogEntryType.Information;
            }
        }
    }
}
