// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Threading {
    /// <summary>
    /// BinaryAsyncLock is a helper primitive that can be used instead of SemaphoreSlim.WaitAsync + double-checked locking
    /// After BinaryAsyncLock is created or reset, the first caller of <see cref="WaitAsync"/> will immediately get <see langword="false" />
    /// All other callers will wait until <see cref="Release"/> is called and then will get <see langword="true" />
    /// </summary>
    public class BinaryAsyncLock {
        private TaskCompletionSource<bool> _tcs;

        public Task<bool> WaitAsync() {
            var tcs = Interlocked.CompareExchange(ref _tcs, new TaskCompletionSource<bool>(), null);
            return tcs != null ? tcs.Task : Task.FromResult(false);
        }

        public void Reset() {
            var tcs = Interlocked.Exchange(ref _tcs, null);
            tcs?.TrySetCanceled();
        }

        public bool ResetIfNotWaiting() {
            var tcs = _tcs;
            if (tcs == null) {
                return true;
            }

            if (tcs.Task.IsCompleted) {
                var oldTcs = Interlocked.CompareExchange(ref _tcs, null, tcs);
                if (oldTcs == tcs) {
                    tcs.TrySetCanceled();
                    return true;
                }
            }

            return false;
        }

        public void Release() {
            _tcs.TrySetResult(true);
        }

        public bool IsCompleted => _tcs?.Task.IsCompleted ?? false;
    }
}
