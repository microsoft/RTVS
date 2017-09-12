// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using Microsoft.Common.Core.Threading;

namespace Microsoft.R.LanguageServer.Services {
    internal sealed class MainThread : IMainThread {
        private readonly SynchronizationContext _syncContext;

        public MainThread() {
            ThreadId = Thread.CurrentThread.ManagedThreadId;
            _syncContext = SynchronizationContext.Current;
        }

        public int ThreadId { get; }

        public void Post(Action action, CancellationToken cancellationToken = new CancellationToken()) {
            if (ThreadId == Thread.CurrentThread.ManagedThreadId) {
                action();
            } else {
                _syncContext.Post(MainThreadAction, new Tuple<Action, CancellationToken>(action, cancellationToken));
            }
        }

        private static void MainThreadAction(object state) {
            var t = (Tuple<Action, CancellationToken>)state;
            if(t.Item2.IsCancellationRequested) {
                return;
            }
            t.Item1();
        }
    }
}
