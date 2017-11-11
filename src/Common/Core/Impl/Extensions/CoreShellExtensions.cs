// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Core.UI;

namespace Microsoft.Common.Core.Shell {
    public static class CoreShellExtensions {
        public static MainThreadAwaitable SwitchToMainThreadAsync(this ICoreShell coreShell, CancellationToken cancellationToken = default(CancellationToken))
             => coreShell.MainThread().SwitchToAsync(cancellationToken);

        public static async Task ShowErrorMessageAsync(this ICoreShell coreShell, string message, CancellationToken cancellationToken = default(CancellationToken)) {
            await coreShell.SwitchToMainThreadAsync(cancellationToken);
            coreShell.UI().ShowErrorMessage(message);
        }

        public static async Task<MessageButtons> ShowMessageAsync(this IServiceContainer services, string message, MessageButtons buttons, CancellationToken cancellationToken = default(CancellationToken)) {
            await services.MainThread().SwitchToAsync(cancellationToken);
            return services.UI().ShowMessage(message, buttons);
        }

        [Conditional("TRACE")]
        public static void AssertIsOnMainThread(this ICoreShell coreShell, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0) {
            if (coreShell.MainThread().ThreadId != Thread.CurrentThread.ManagedThreadId) {
                Debug.Fail(FormattableString.Invariant($"{memberName} at {sourceFilePath}:{sourceLineNumber} was incorrectly called from a background thread."));
            }
        }
    }
}
