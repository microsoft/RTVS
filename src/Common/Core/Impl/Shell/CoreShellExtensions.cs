// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Threading;

namespace Microsoft.Common.Core.Shell {
    public static class CoreShellExtensions {
        public static MainThreadAwaitable SwitchToMainThreadAsync(this ICoreShell coreShell, CancellationToken cancellationToken = default(CancellationToken))
             => ((IMainThread)coreShell).SwitchToAsync(cancellationToken);

        public static async Task ShowErrorMessageAsync(this ICoreShell coreShell, string message, CancellationToken cancellationToken = default(CancellationToken)) {
            await coreShell.SwitchToMainThreadAsync(cancellationToken);
            coreShell.ShowErrorMessage(message);
        }

        public static async Task<MessageButtons> ShowMessageAsync(this ICoreShell coreShell, string message, MessageButtons buttons, CancellationToken cancellationToken = default(CancellationToken)) {
            await coreShell.SwitchToMainThreadAsync(cancellationToken);
            return coreShell.ShowMessage(message, buttons);
        }

        [Conditional("TRACE")]
        public static void AssertIsOnMainThread(this ICoreShell coreShell, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0) {
            if (coreShell.MainThread != Thread.CurrentThread) {
                Trace.Fail(FormattableString.Invariant($"{memberName} at {sourceFilePath}:{sourceLineNumber} was incorrectly called from a background thread."));
            }
        }
    }
}
