// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Text;
using Microsoft.Common.Core.Logging;
using static System.FormattableString;

namespace Microsoft.R.Host.Client {
    internal static class RHostLoggingExtensions {
        public static void RHostProcessStarted(this IActionLog log, ProcessStartInfo psi) {
            var sb = new StringBuilder();
            sb.AppendLine(Invariant($"R Host process started: {psi.FileName}"));
            if (psi.EnvironmentVariables.Count > 0) {
                sb.AppendLine("Environment variables:");
                foreach (var variable in psi.Environment) {
                    sb.Append(' ', 4).AppendLine(Invariant($"{variable.Key}={variable.Value}"));
                }
            }
            log.WriteLineAsync(MessageCategory.General, sb.ToString());
        }

        public static void RHostProcessExited(this IActionLog log) {
            log.WriteLineAsync(MessageCategory.General, "R Host process exited");
        }

        public static void ConnectedToRHostWebSocket(this IActionLog log, string uri, int attempt) {
            log.WriteLineAsync(MessageCategory.General, Invariant($"Connected to R Web Host socket at {uri} on attempt #{attempt}"));
        }

        public static void FailedToConnectToRHost(this IActionLog log) {
            log.WriteLineAsync(MessageCategory.General, Invariant($"Timed out waiting for RHost to connect"));
        }

        public static void EnterRLoop(this IActionLog log, int depth) {
            log.WriteLineAsync(MessageCategory.General, Invariant($"Enter R loop, depth={depth}"));
        }

        public static void ExitRLoop(this IActionLog log, int depth) {
            log.WriteLineAsync(MessageCategory.General, Invariant($"Exit R loop, depth={depth}"));
        }

        public static void Request(this IActionLog log, string request, int depth) {
            log.WriteLineAsync(MessageCategory.General, Invariant($"[Request,depth={depth}]:{request}"));
        }

        public static void Response(this IActionLog log, string response, int depth) {
            log.WriteLineAsync(MessageCategory.General, Invariant($"[Response,depth={depth}]:{response}"));
        }
    }
}