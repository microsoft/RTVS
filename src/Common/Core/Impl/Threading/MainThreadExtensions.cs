// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Threading {
    public static class MainThreadExtensions {
        public static MainThreadAwaitable SwitchToAsync(this IMainThread mainThread, CancellationToken cancellationToken = default(CancellationToken))
            => new MainThreadAwaitable(mainThread, cancellationToken);

        public static bool CheckAccess(this IMainThread mainThread)
            => Thread.CurrentThread.ManagedThreadId == mainThread.ThreadId;


        [Conditional("TRACE")]
        public static void Assert(this IMainThread mainThread, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0) {
            if (mainThread.ThreadId != Thread.CurrentThread.ManagedThreadId) {
                Debug.Fail(FormattableString.Invariant($"{memberName} at {sourceFilePath}:{sourceLineNumber} was incorrectly called from a background thread."));
            }
        }

        public static void ExecuteOrPost(this IMainThread mainThread, Action action) {
            if (mainThread.CheckAccess()) {
                action();
            } else {
                mainThread.Post(action);
            }
        }

        public static async Task SendAsync(this IMainThread mainThread, Action action, CancellationToken cancellationToken = default(CancellationToken)) {
            await mainThread.SwitchToAsync(cancellationToken);
            action();
        }

        public static async Task<T> SendAsync<T>(this IMainThread mainThread, Func<T> action, CancellationToken cancellationToken = default(CancellationToken)) {
            await mainThread.SwitchToAsync(cancellationToken);
            return action();
        }
    }
}