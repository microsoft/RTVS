// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Common.Core.Diagnostics;

namespace Microsoft.Common.Core.Logging {
    /// <summary>
    /// Represents OS Application event log
    /// </summary>
    public sealed class ApplicationLogWriter : IActionLogWriter, IDisposable {
        private const string _root = @"Microsoft\R Tools\";
        private readonly string _applicationName;
        private readonly EventLog _eventLog;

        public ApplicationLogWriter(string applicationName) {
            Check.ArgumentStringNullOrEmpty(nameof(applicationName), applicationName);

            var source = _root + applicationName;
            _eventLog = new EventLog(_applicationName, source);
        }

        public void Dispose() {
            _eventLog?.Dispose();
        }


        public void Flush() { }

        public Task WriteAsync(MessageCategory category, string message) {
            _eventLog.WriteEntry(message, ToEntryType(category));
            return Task.CompletedTask;
        }

        private static EventLogEntryType ToEntryType(MessageCategory category) {
            switch (category) {
                case MessageCategory.Error:
                    return EventLogEntryType.Error;
                case MessageCategory.Warning:
                    return EventLogEntryType.Warning;
            }
            return EventLogEntryType.Information;
        }
    }
}
