// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Logging {
    /// <summary>
    /// Application event logger
    /// </summary>
    internal sealed class Logger : IActionLog, IDisposable {
        private IActionLogWriter[] _logs;
        private readonly LogLevel _maxLogLevel;
        private readonly string _appName;
        private readonly IActionLogWriter _writer;

        public void Dispose() {
            foreach (var log in _logs) {
                (log as IDisposable)?.Dispose();
            }
        }

        internal Logger(string appName, LogLevel maxLogLevel, IActionLogWriter writer) {
            _appName = appName;
            _maxLogLevel = maxLogLevel;
            _writer = writer;
        }

        private void EnsureCreated() {
            // Delay-create log since permission is established when settings are loaded
            // which may happen after ctor is called.
            if (_logs == null) {
                _logs = new IActionLogWriter[Enum.GetValues(typeof(LogLevel)).Length];
                _logs[(int)LogLevel.None] = NullLogWriter.Instance;
                _logs[(int)LogLevel.Minimal] = _maxLogLevel >= LogLevel.Minimal ? (_writer ?? new ApplicationLogWriter(_appName)) : NullLogWriter.Instance;
                _logs[(int)LogLevel.Normal] = _maxLogLevel >= LogLevel.Normal ? (_writer ?? FileLogWriter.InTempFolder(_appName)) : NullLogWriter.Instance;
                _logs[(int)LogLevel.Traffic] = _maxLogLevel == LogLevel.Traffic ? (_writer ?? FileLogWriter.InTempFolder(_appName + ".traffic")) : NullLogWriter.Instance;
            }
        }

        #region IActionLog
        public Task WriteAsync(LogLevel logLevel, MessageCategory category, string message) {
            EnsureCreated();
            return _logs[(int)logLevel].WriteAsync(category, message);
        }
        public Task WriteFormatAsync(LogLevel logLevel, MessageCategory category, string format, params object[] arguments) {
            EnsureCreated();
            string message = string.Format(CultureInfo.InvariantCulture, format, arguments);
            return _logs[(int)logLevel].WriteAsync(category, message);
        }
        public Task WriteLineAsync(LogLevel logLevel, MessageCategory category, string message) {
            EnsureCreated();
            return _logs[(int)logLevel].WriteAsync(category, message + Environment.NewLine);
        }

        public void Flush() {
            foreach (var l in _logs) {
                l?.Flush();
            }
        }
        #endregion
    }
}
