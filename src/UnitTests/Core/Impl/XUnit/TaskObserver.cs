// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Threading;

namespace Microsoft.UnitTests.Core.XUnit {
    internal class TaskObserver : IDisposable {
        private readonly Action _onDispose;
        private readonly Action<Task> _afterTaskCompleted;
        private readonly TaskCompletionSource<Exception> _tcs;
        private readonly ConcurrentDictionary<int, string> _stackTraces;
        private int _count;
        private bool _isTestCompleted;

        public Task<Exception> Task => _tcs.Task;

        public TaskObserver(Action onDispose) {
            _onDispose = onDispose;
            _afterTaskCompleted = AfterTaskCompleted;
            _tcs = new TaskCompletionSource<Exception>();
            _stackTraces = new ConcurrentDictionary<int, string>();
        }

        public void Add(Task task) {
            Interlocked.Increment(ref _count);
            _stackTraces.TryAdd(task.Id, new StackTrace(2).ToString());
            task.ContinueWith(_afterTaskCompleted, TaskContinuationOptions.ExecuteSynchronously);
        }

        public void TestCompleted() {
            Volatile.Write(ref _isTestCompleted, true);
            if (_count == 0) {
                _tcs.TrySetResult(null);
            }
        }

        private void AfterTaskCompleted(Task task) {
            var count = Interlocked.Decrement(ref _count);
            _stackTraces.TryRemove(task.Id, out _);

            if (task.IsFaulted) {
                _tcs.TrySetException(task.Exception);
            } else if (task.IsCompleted && count == 0 && Volatile.Read(ref _isTestCompleted)) {
                _tcs.TrySetResult(null);
            }
        }

        public void Dispose() => _onDispose();
    }
}