// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.UnitTests.Core.XUnit {
    internal class TaskObserver : IDisposable {
        private readonly ITestMainThreadFixture _testMainThreadFixture;
        private readonly Action _onDispose;
        private readonly Action<Task, object> _afterTaskCompleted;
        private readonly TaskCompletionSource<Exception> _tcs;
        private readonly ConcurrentDictionary<int, string> _stackTraces;
        private int _count;
        private bool _isTestCompleted;

        public Task<Exception> Task => _tcs.Task;

        public TaskObserver(ITestMainThreadFixture testMainThreadFixture, Action onDispose) {
            _testMainThreadFixture = testMainThreadFixture;
            _onDispose = onDispose;
            _afterTaskCompleted = AfterTaskCompleted;
            _tcs = new TaskCompletionSource<Exception>();
            _stackTraces = new ConcurrentDictionary<int, string>();
        }

        public void Add(Task task) {
            Interlocked.Increment(ref _count);
            _stackTraces.TryAdd(task.Id, new StackTrace(2).ToString());
            var postToMainThread = _testMainThreadFixture.CheckAccess();
            task.ContinueWith(_afterTaskCompleted, postToMainThread, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public void TestCompleted() {
            Volatile.Write(ref _isTestCompleted, true);
            if (_count == 0) {
                _tcs.TrySetResult(null);
            }
        }

        private void AfterTaskCompleted(Task task, object state) {
            var count = Interlocked.Decrement(ref _count);
            _stackTraces.TryRemove(task.Id, out _);

            if (task.IsFaulted) {
                var aggregateException = task.Exception.Flatten();
                var exception = aggregateException.InnerExceptions.Count == 1
                    ? aggregateException.InnerException
                    : aggregateException;

                var postToMainThread = (bool)state;
                if (postToMainThread) {
                    _testMainThreadFixture.Post(ReThrowTaskException, exception);
                } else {
                    _tcs.TrySetException(exception);
                }
            } else if (task.IsCompleted && count == 0 && Volatile.Read(ref _isTestCompleted)) {
                _tcs.TrySetResult(null);
            }
        }

        private static void ReThrowTaskException(object state) => ExceptionDispatchInfo.Capture((Exception)state).Throw();

        public void Dispose() => _onDispose();
    }
}