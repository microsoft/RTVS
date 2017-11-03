// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Common.Core.Diagnostics;

namespace Microsoft.Common.Core.Threading {
    public struct MainThreadAwaiter : INotifyCompletion {
        private readonly IMainThread _mainThread;
        private readonly IMainThreadAwaiter _awaiterImpl;

        public MainThreadAwaiter(IMainThread mainThread, CancellationToken cancellationToken) {
            Check.ArgumentNull(nameof(mainThread), mainThread);
            _mainThread = mainThread;
            _awaiterImpl = _mainThread.CreateMainThreadAwaiter(cancellationToken);
        }

        public bool IsCompleted => _awaiterImpl.IsCompleted;

        public void OnCompleted(Action continuation) {
            Trace.Assert(continuation != null);
            _awaiterImpl.OnCompleted(continuation);
        }

        public void GetResult() => _awaiterImpl.GetResult();
    }
}