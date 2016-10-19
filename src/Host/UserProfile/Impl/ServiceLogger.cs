// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Common.Core.Disposables;
using Microsoft.Extensions.Logging;

namespace Microsoft.R.Host.UserProfile {
    internal sealed class ServiceLogger : ILogger, IDisposable {
        private readonly string _category;
        private volatile StreamWriter _writer;

        public ServiceLogger(string category, StreamWriter writer) {
            _category = category;
            _writer = writer;
        }

        public void Dispose() {
            _writer = null;
        }

        public IDisposable BeginScope<TState>(TState state) {
            return Disposable.Empty;
        }

        public bool IsEnabled(LogLevel logLevel) {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
            var writer = _writer;
            if (writer == null) {
                return;
            }

            string message = formatter(state, exception);
            lock (writer) {
                writer.WriteLine("[{0:u}] <{1}> ({2}):", DateTime.Now, _category, logLevel.ToString()[0]);
                writer.WriteLine(message);

                if (exception != null) {
                    writer.WriteLine("Exception: " + exception);
                }

                writer.WriteLine();
                writer.Flush();
                using (EventLog eventLog = new EventLog("Application")) {
                    EventLogEntryType logType = GetLogType(logLevel);
                    eventLog.Source = "Application";
                    string logMessage;
                    if (exception != null) {
                        logMessage = string.Format("[{0:u}] <{1}> ({2}), Message: {3}, Exception: {4}", DateTime.Now, _category, logLevel.ToString()[0], message, exception);
                    } else {
                        logMessage = string.Format("[{0:u}] <{1}> ({2}), Message: {3}", DateTime.Now, _category, logLevel.ToString()[0], message);
                    }
                    eventLog.WriteEntry(logMessage, logType);
                }
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
