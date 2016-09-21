// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Logging {
    /// <summary>
    /// Application event logger
    /// </summary>
    public sealed class Logger : IActionLog {
        private static Logger _instance;

        private readonly IActionLogWriter[] _logs = new IActionLogWriter[Enum.GetValues(typeof(LogLevel)).Length];
        private readonly LogLevel _details;

        public static IActionLog Current {
            get {
                if (_instance == null) {
                    throw new InvalidOperationException("Log is not open yet");
                }
                return _instance;
            }
        }

        public static void Open(string appName, LogLevel details) {
            if (_instance != null) {
                throw new InvalidOperationException("Log is already open");
            }
            _instance = new Logger(appName, details);
        }

        public static void Close() {
            foreach(var log in _instance._logs) {
                (log as IDisposable)?.Dispose();
            }
        }

        internal Logger(string appName, LogLevel details) {
            _details = details;

            _logs[(int)LogLevel.None] = NullLogWriter.Instance;
            _logs[(int)LogLevel.Minimal] = _details >= LogLevel.Minimal ? new ApplicationLogWriter(appName) : NullLogWriter.Instance;
            _logs[(int)LogLevel.Normal] = _details >= LogLevel.Normal ? FileLogWriter.InTempFolder(appName) : NullLogWriter.Instance;
            _logs[(int)LogLevel.Traffic] = details == LogLevel.Traffic ? FileLogWriter.InTempFolder(appName + ".traffic") : NullLogWriter.Instance;
        }

        #region IActionLog
        public Task WriteAsync(LogLevel logLevel, MessageCategory category, string message) {
            return _logs[(int)logLevel].WriteAsync(category, message);
        }
        public Task WriteFormatAsync(LogLevel logLevel, MessageCategory category, string format, params object[] arguments) {
            string message = string.Format(CultureInfo.InvariantCulture, format, arguments);
            return _logs[(int)logLevel].WriteAsync(category, message);
        }
        public Task WriteLineAsync(LogLevel logLevel, MessageCategory category, string message) {
            return _logs[(int)logLevel].WriteAsync(category, message + Environment.NewLine);
        }
        #endregion
    }
}
