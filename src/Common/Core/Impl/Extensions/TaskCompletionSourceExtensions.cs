// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core {
    public static class TaskCompletionSourceExtensions {
        public static CancellationTokenRegistration RegisterForCancellation<T>(this TaskCompletionSource<T> taskCompletionSource, CancellationToken cancellationToken) 
            => taskCompletionSource.RegisterForCancellation(-1, cancellationToken);

        public static CancellationTokenRegistration RegisterForCancellation<T>(this TaskCompletionSource<T> taskCompletionSource, int millisecondsDelay, CancellationToken cancellationToken) {
            if (millisecondsDelay >= 0) {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(millisecondsDelay);
                cancellationToken = cts.Token;
            }

            var action = new CancelOnTokenAction<T>(taskCompletionSource, cancellationToken);
            return cancellationToken.Register(action.Invoke);
        }

        private struct CancelOnTokenAction<T> {
            private readonly TaskCompletionSource<T> _taskCompletionSource;
            private readonly CancellationToken _cancellationToken;

            public CancelOnTokenAction(TaskCompletionSource<T> taskCompletionSource, CancellationToken cancellationToken) {
                _taskCompletionSource = taskCompletionSource;
                _cancellationToken = cancellationToken;
            }

            public void Invoke() {
                if (!_taskCompletionSource.Task.IsCompleted) {
                    ThreadPool.QueueUserWorkItem(TryCancel);
                }
            }

            private void TryCancel(object state) => _taskCompletionSource.TrySetCanceled(_cancellationToken);
        }
    }
}