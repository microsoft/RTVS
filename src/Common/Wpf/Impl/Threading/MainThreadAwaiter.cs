// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Threading;

namespace Microsoft.Common.Wpf.Threading {
    public struct MainThreadAwaiter : ICriticalNotifyCompletion {
        private readonly Dispatcher _dispatcher;
        private static readonly Action<Action> Callback = a => a();

        public MainThreadAwaiter(Dispatcher dispatcher) {
            _dispatcher = dispatcher;
        }

        public bool IsCompleted => Thread.CurrentThread == _dispatcher.Thread;

        public void OnCompleted(Action continuation) {
            Trace.Assert(continuation != null);
            _dispatcher.BeginInvoke(DispatcherPriority.Normal, Callback, continuation);
        }

        public void UnsafeOnCompleted(Action continuation) {
            Trace.Assert(continuation != null);
            _dispatcher.BeginInvoke(DispatcherPriority.Normal, Callback, continuation);
        }

        public void GetResult() {
            if (Thread.CurrentThread != _dispatcher.Thread) {
                throw new InvalidOperationException();
            }
        }
    }
}