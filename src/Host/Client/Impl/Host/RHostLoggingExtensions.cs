// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Logging;
using static System.FormattableString;

namespace Microsoft.R.Host.Client {
    internal static class RHostLoggingExtensions {
        public static void RHostProcessExited(this IActionLog log)
            => log.WriteLine(LogVerbosity.Minimal, MessageCategory.General, "R Host process exited");

        public static void ConnectedToRHostWebSocket(this IActionLog log, string uri, int attempt)
            => log.WriteLine(LogVerbosity.Minimal, MessageCategory.General, Invariant($"Connected to R Web Host socket at {uri} on attempt #{attempt}"));

        public static void FailedToConnectToRHost(this IActionLog log)
            => log.WriteLine(LogVerbosity.Minimal, MessageCategory.General, Invariant($"Timed out waiting for RHost to connect"));

        public static void EnterRLoop(this IActionLog log, int depth)
            => log.WriteLine(LogVerbosity.Normal, MessageCategory.General, Invariant($"Enter R loop, depth={depth}"));

        public static void ExitRLoop(this IActionLog log, int depth)
            => log.WriteLine(LogVerbosity.Normal, MessageCategory.General, Invariant($"Exit R loop, depth={depth}"));

        public static void Request(this IActionLog log, string request, int depth)
            => log.WriteLine(LogVerbosity.Traffic, MessageCategory.General, Invariant($"[Request,depth={depth}]:{request}"));

        public static void Response(this IActionLog log, string response, int depth)
            => log.WriteLine(LogVerbosity.Traffic, MessageCategory.General, Invariant($"[Response,depth={depth}]:{response}"));
    }
}