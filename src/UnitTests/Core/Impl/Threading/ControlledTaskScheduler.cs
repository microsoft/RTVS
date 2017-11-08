// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;

namespace Microsoft.UnitTests.Core.Threading {
    public class ControlledTaskScheduler : TaskScheduler {
        private readonly SynchronizationContext _syncContext;
        private readonly SendOrPostCallback _callback;
        private readonly ConcurrentQueue<Task> _pendingTasks;

        private volatile bool _paused;
        private int _scheduledTasksCount;
        private TaskCompletionSource<object> _futureTaskWaitingCompletionSource;
        private TaskCompletionSource<object> _emptyQueueCompletionSource;
        private List<Exception> _exceptions;
        private Task _emptyQueueTask;

        public override int MaximumConcurrencyLevel => 1;

        public int ScheduledTasksCount => _scheduledTasksCount;

        public ControlledTaskScheduler(SynchronizationContext syncContext) {
            _syncContext = syncContext;
            _pendingTasks = new ConcurrentQueue<Task>();
            _callback = obj => Callback((Task)obj);

            _futureTaskWaitingCompletionSource = new TaskCompletionSource<object>();
            _emptyQueueCompletionSource = null;
            _exceptions = new List<Exception>();
            _emptyQueueTask = Task.CompletedTask;
        }

        public TaskAwaiter GetAwaiter() {
            return _emptyQueueTask.GetAwaiter();
        }

        /// <summary>
        /// Waits until scheduler queue is empty
        /// </summary>
        public void Wait() {
            _emptyQueueTask.Wait();
        }

        /// <summary>
        /// Waits for tasks to be scheduled and then until scheduler queue is empty
        /// If there are tasks in the queue already, behaves similar to Wait().
        /// </summary>
        /// <param name="ms">
        /// Number of milliseconds to wait until task is scheduled.
        /// Should be greater than 0.
        /// Default value is 1000ms.
        /// </param>
        public void WaitForUpcomingTasks(int ms = 1000) {
            if (ms <= 0) {
                throw new ArgumentException(@"Number of milliseconds to wait should be positive", nameof(ms));
            }

            if (!_futureTaskWaitingCompletionSource.Task.Wait(ms)) {
                throw new TimeoutException();
            }

            _emptyQueueTask.Wait();
        }

        public IDisposable Pause() {
            _paused = true;
            return Disposable.Create(Resume);
        }

        private void Resume() {
            _paused = false;
            while (_pendingTasks.TryPeek(out var task)) {
                _syncContext.Post(_callback, task);
                _pendingTasks.TryDequeue(out task);
            }
        }

        [SecurityCritical]
        protected override void QueueTask(Task task) {
            if (Interlocked.Increment(ref _scheduledTasksCount) == 1) {
                var spinWait = new SpinWait();
                var tcs = new TaskCompletionSource<object>();
                while (Interlocked.CompareExchange(ref _emptyQueueCompletionSource, tcs, null) != null) {
                    spinWait.SpinOnce();
                }

                _emptyQueueTask = _emptyQueueCompletionSource.Task;
                _futureTaskWaitingCompletionSource.SetResult(null);
            }

            if (_pendingTasks.Count > 0 || _paused) {
                _pendingTasks.Enqueue(task);
            } else {
                _syncContext.Post(_callback, task);
            }
        }

        [SecurityCritical]
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) {
            return SynchronizationContext.Current == _syncContext && TryExecuteTask(task);
        }

        [SecurityCritical]
        protected override IEnumerable<Task> GetScheduledTasks() {
            return null;
        }

        private void Callback(Task task) {
            try {
                TryExecuteTask(task);
            } finally {
                AfterTaskExecuted(task);
            }
        }

        private void AfterTaskExecuted(Task task) {
            if (task.IsFaulted && task.Exception != null) {
                _exceptions.AddRange(task.Exception.InnerExceptions);
            }

            if (Interlocked.Decrement(ref _scheduledTasksCount) == 0) {
                Interlocked.Exchange(ref _futureTaskWaitingCompletionSource, new TaskCompletionSource<object>());
                var exceptions = Interlocked.Exchange(ref _exceptions, new List<Exception>());
                var tcs = Interlocked.Exchange(ref _emptyQueueCompletionSource, null);

                if (exceptions.Any()) {
                    tcs.SetException(exceptions);
                } else {
                    tcs.SetResult(null);
                }
            }
        }
    }
}
