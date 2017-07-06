// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;

namespace Microsoft.Common.Core.Threading {
    public static class MainThreadExtensions {
        public static MainThreadAwaitable SwitchToAsync(this IMainThread mainThread, CancellationToken cancellationToken = default(CancellationToken))
            => new MainThreadAwaitable(mainThread, cancellationToken);

        public static bool CheckAccess(this IMainThread mainThread)
            => Thread.CurrentThread.ManagedThreadId == mainThread.ThreadId;

        public static void ExecuteOrPost(this IMainThread mainThread, Action action, CancellationToken cancellationToken = default(CancellationToken)) {
            if (mainThread.CheckAccess()) {
                action();
            } else {
                mainThread.Post(action, cancellationToken);
            }
        }
    }
}