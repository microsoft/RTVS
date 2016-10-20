// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Common.Core.Disposables;
using Microsoft.Extensions.Logging;

namespace Microsoft.R.Host.UserProfile {
    internal sealed class ServiceLogger : ILogger, IDisposable {
        private readonly string _category;

        public ServiceLogger(string category) {
            _category = category;
        }

        public void Dispose() {
        }

        public IDisposable BeginScope<TState>(TState state) {
            return Disposable.Empty;
        }

        public bool IsEnabled(LogLevel logLevel) {
            return true;
        }


        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
            string message = formatter(state, exception);
            using (EventLog eventLog = new EventLog("Application")) {
                EventLogEntryType logType = GetLogType(logLevel);
                eventLog.Source = "Application";
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
