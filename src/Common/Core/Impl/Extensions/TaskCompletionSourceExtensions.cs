// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core {
    public static class TaskCompletionSourceExtensions {
        public static CancellationTokenRegistration RegisterForCancellation<T>(this TaskCompletionSource<T> taskCompletionSource, CancellationToken cancellationToken) {
            var action = new CancelOnTokenAction<T>(taskCompletionSource, cancellationToken);
            return cancellationToken.Register(action.Invoke);
        }

        private class CancelOnTokenAction<T> {
            private readonly TaskCompletionSource<T> _taskCompletionSource;
            private readonly CancellationToken _cancellationToken;

            public CancelOnTokenAction(TaskCompletionSource<T> taskCompletionSource, CancellationToken cancellationToken) {
                _taskCompletionSource = taskCompletionSource;
                _cancellationToken = cancellationToken;
            }

            public void Invoke() {
                if (!_taskCompletionSource.Task.IsCompleted) {
                    Task.Run(new Action(TryCancel));
                }
            }

            private void TryCancel() => _taskCompletionSource.TrySetCanceled(_cancellationToken);
        }
    }
}