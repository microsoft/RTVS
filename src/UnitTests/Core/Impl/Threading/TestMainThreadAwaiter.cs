// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using Microsoft.Common.Core.Threading;

namespace Microsoft.UnitTests.Core.Threading {
    internal class TestMainThreadAwaiter : IMainThreadAwaiter {
        private static readonly WaitCallback ThreadPoolCallback = state => ((ContinuationWrapper)state).Invoke();
        // In case of CancellationToken, we need to call ContinuationWrapper.Invoke as a separate item, 
        // otherwise in case of linked CancellationToken tree it may lead to a deadlock.
        private static readonly Action<object> CancellationTokenCallback = state => ThreadPool.QueueUserWorkItem(ThreadPoolCallback, state);

        private readonly IMainThread _mainThread;
        private readonly CancellationToken _cancellationToken;

        public TestMainThreadAwaiter(IMainThread mainThread, CancellationToken cancellationToken) {
            _mainThread = mainThread;
            _cancellationToken = cancellationToken;
        }

        public bool IsCompleted => Thread.CurrentThread.ManagedThreadId == _mainThread.ThreadId;

        public void OnCompleted(Action continuation) {
            var wrapper = new ContinuationWrapper(continuation);

            if (_cancellationToken.IsCancellationRequested) {
                ThreadPool.QueueUserWorkItem(ThreadPoolCallback, wrapper);
                return;
            }

            _mainThread.Post(wrapper.Invoke);
            var registration = _cancellationToken.Register(CancellationTokenCallback, wrapper, false);

            var disposeRegistration = false;
            lock (wrapper) {
                // wrapper.Action == null means that ContinuationWrapper.Invoke has been called before CancellationTokenRegistration was set
                // In this case, registration should be disposed explicitly
                if (wrapper.Action == null) {
                    disposeRegistration = true;
                } else {
                    wrapper.CancellationTokenRegistration = registration;
                }
            }

            if (disposeRegistration) {
                registration.Dispose();
            }
        }

        public void GetResult() {
            if (Thread.CurrentThread.ManagedThreadId != _mainThread.ThreadId) {
                _cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private class ContinuationWrapper {
            public Action Action;
            public CancellationTokenRegistration CancellationTokenRegistration;

            public ContinuationWrapper(Action action) {
                Action = action;
            }

            public void Invoke() {
                Action action;
                CancellationTokenRegistration registration;

                lock (this) {
                    action = Action;
                    registration = CancellationTokenRegistration;
                    Action = null;
                }

                if (action != null) {
                    registration.Dispose();
                    action.Invoke();
                }
            }
        }
    }
}