// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Threading {
    /// <summary>
    /// BinaryAsyncLock is a helper primitive that can be used instead of SemaphoreSlim.WaitAsync + double-checked locking
    /// After BinaryAsyncLock is created or reset, the first caller of <see cref="WaitAsync"/> will immediately get <see langword="false" />
    /// All other callers will wait until <see cref="Release"/> is called and then will get <see langword="true" />
    /// </summary>
    public class BinaryAsyncLock {
        private static readonly Task<bool> FalseTask = Task.FromResult(false);
        private static readonly Task<bool> TrueTask = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> _queue = new Queue<TaskCompletionSource<bool>>();
        private volatile bool _isCompleted;

        public Task<bool> WaitAsync() {
            TaskCompletionSource<bool> tcs;
            bool isFirst;
            lock (_queue) {
                if (_isCompleted) {
                    return TrueTask;
                }

                tcs = new TaskCompletionSource<bool>();
                isFirst = _queue.Count == 0;
                _queue.Enqueue(tcs);
            }

            if (isFirst) {
                tcs.SetResult(false);
            }

            return tcs.Task;
        }

        public Task<bool> WaitIfLockedAsync() {
            lock (_queue) {
                if (_isCompleted) {
                    return TrueTask;
                }

                if (_queue.Count == 0) {
                    return FalseTask;
                }

                var tcs = new TaskCompletionSource<bool>();
                _queue.Enqueue(tcs);
                return tcs.Task;
            }
        }

        public void Reset() {
            TaskCompletionSource<bool> tcs = null;
            lock (_queue) {
                if (_queue.Count == 0) {
                    return;
                }

                _queue.Dequeue();
                if (_queue.Count > 0) {
                    tcs = _queue.Peek();
                }
            }
            tcs?.SetResult(false);
        }

        public bool ResetIfNotWaiting() {
            TaskCompletionSource<bool> tcs = null;
            lock (_queue) {
                if (_queue.Count > 0) {
                    var current = _queue.Dequeue();
                    if (!current.Task.IsCompleted) {
                        return false;
                    }

                    if (_queue.Count > 0) {
                        tcs = _queue.Peek();
                    }
                }
            }

            tcs?.SetResult(false);
            _isCompleted = false;
            return true;
        }

        public void Release() {
            TaskCompletionSource<bool>[] queue;
            lock (_queue) {
                queue = _queue.ToArray();
                _queue.Clear();
                _isCompleted = true;
            }

            foreach (var tcs in queue) {
                tcs.TrySetResult(true);
            }
        }

        public bool IsCompleted => _isCompleted;
    }
}
