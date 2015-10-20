using System;
using System.Diagnostics;
using System.Text;
using Microsoft.R.Actions.Logging;
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

        public static void ConnectedToRHostWebSocket(this IActionLog log, Uri uri, int attempt) {
            log.WriteLineAsync(MessageCategory.General, Invariant($"Connected to R Web Host socket at {uri} on attempt #{attempt}"));
        }

        public static void FailedToConnectToRHost(this IActionLog log, Uri uri) {
            log.WriteLineAsync(MessageCategory.General, Invariant($"Failed to connect to R Host at {uri}"));
        }

        public static void EnterRLoop(this IActionLog log, int depth) {
            log.WriteLineAsync(MessageCategory.General, Invariant($"Enter R loop, depth={depth}"));
        }

        public static void ExitRLoop(this IActionLog log, int depth) {
            log.WriteLineAsync(MessageCategory.General, Invariant($"Exit R loop, depth={depth}"));
        }

        public static void Request(this IActionLog log, string request, int depth, ulong id) {
            log.WriteLineAsync(MessageCategory.General, Invariant($"[Request,depth={depth},id={id}]:{request}"));
        }

        public static void Response(this IActionLog log, string response, int depth, ulong id) {
            log.WriteLineAsync(MessageCategory.General, Invariant($"[Response,depth={depth},id={id}]:{response}"));
        }
    }
}