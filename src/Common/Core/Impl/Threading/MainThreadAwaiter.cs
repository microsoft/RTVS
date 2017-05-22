// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Common.Core.Diagnostics;

namespace Microsoft.Common.Core.Threading {
    public struct MainThreadAwaiter : ICriticalNotifyCompletion {
        private readonly IMainThread _mainThread;
        private readonly CancellationToken _cancellationToken;

        public MainThreadAwaiter(IMainThread mainThread, CancellationToken cancellationToken) {
            Check.ArgumentNull(nameof(mainThread), mainThread);
            _mainThread = mainThread;
            _cancellationToken = cancellationToken;
        }

        public bool IsCompleted => Thread.CurrentThread.ManagedThreadId == _mainThread.ThreadId;

        public void OnCompleted(Action continuation) {
            Trace.Assert(continuation != null);
            _mainThread.Post(continuation, _cancellationToken);
        }

        public void UnsafeOnCompleted(Action continuation) {
            Trace.Assert(continuation != null);
            _mainThread.Post(continuation, _cancellationToken);
        }

        public void GetResult() {
            if (Thread.CurrentThread.ManagedThreadId != _mainThread.ThreadId) {
                _cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}