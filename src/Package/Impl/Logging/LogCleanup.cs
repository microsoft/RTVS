// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.Logging {
    internal static class LogCleanup {
        private static CancellationTokenSource _cts;

        public static void Cancel() {
            if (_cts != null) {
                _cts.Cancel();
                _cts = null;
            }
        }

        /// <summary>
        /// Deletes logs that are too old asynchronously.
        /// Not designed to be called multiple times.
        /// Typically called once when package is loaded.
        /// </summary>
        /// <param name="daysOlderThan">Logs older than this number of days will be deleted</param>
        public static Task DeleteLogsAsync(int daysOlderThan) {
            _cts = new CancellationTokenSource();
            return Task.Run(() => DeleteLogs(daysOlderThan, _cts.Token), _cts.Token);
        }

        /// <summary>
        /// Deletes logs that are too old
        /// </summary>
        /// <param name="daysOlderThan">Logs older than this number of days will be deleted</param>
        private static void DeleteLogs(int daysOlderThan, CancellationToken ct) {
            try {
                string tempPath = Path.GetTempPath();
                DateTime now = DateTime.Now.ToUniversalTime();

                foreach (var pattern in DiagnosticLogs.RtvsLogFilePatterns) {
                    ct.ThrowIfCancellationRequested();

                    IEnumerable<string> oldFiles = Directory.EnumerateFiles(tempPath, pattern).Where((x) => {
                        ct.ThrowIfCancellationRequested();
                        return (now - File.GetLastWriteTimeUtc(x)).TotalDays > daysOlderThan;
                    });

                    foreach (string file in oldFiles) {
                        File.Delete(file);
                    }
                }
            } catch (IOException) {
            } catch (UnauthorizedAccessException) {
            } catch (ArgumentException) {
            }
        }
    }
}
