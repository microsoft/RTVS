using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;

namespace Microsoft.UnitTests.Core.Threading
{
    public class ControlledTaskScheduler : TaskScheduler
    {
        private readonly SynchronizationContext _syncContext;
        private readonly SendOrPostCallback _callback;
        private readonly ConcurrentQueue<Task> _pendingTasks;

        private volatile bool _paused;
        private TaskCompletionSource<object> _futureTaskWaitingCompletionSource;
        private TaskCompletionSource<object> _emptyQueueCompletionSource;
        private Task _emptyQueueTask;

        public override int MaximumConcurrencyLevel => 1;

        public ControlledTaskScheduler(SynchronizationContext syncContext)
        {
            _syncContext = syncContext;
            _pendingTasks = new ConcurrentQueue<Task>();
            _callback = obj => Callback();

            _futureTaskWaitingCompletionSource = new TaskCompletionSource<object>();
            _emptyQueueCompletionSource = null;
            _emptyQueueTask = Task.CompletedTask;
        }

        public TaskAwaiter GetAwaiter()
        {
            return _emptyQueueTask.GetAwaiter();
        }

        /// <summary>
        /// Waits until scheduler queue is empty
        /// </summary>
        public void Wait()
        {
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
        public void WaitForUpcomingTasks(int ms = 1000)
        {
            if (ms <= 0)
            {
                throw new ArgumentException(@"Number of milliseconds to wait should be positive", nameof(ms));
            }

            if (!_futureTaskWaitingCompletionSource.Task.Wait(ms))
            {
                throw new TimeoutException();
            }

            _emptyQueueTask.Wait();
        }

        public IDisposable Pause()
        {
            _paused = true;
            return Disposable.Create(Resume);
        }

        private void Resume()
        {
            _paused = false;
            _syncContext.Post(_callback, null);
        }

        [SecurityCritical]
        protected override void QueueTask(Task task)
        {
            SpinWait spinWait = new SpinWait();
            while (_pendingTasks.IsEmpty)
            {
                if (Interlocked.CompareExchange(ref _emptyQueueCompletionSource, new TaskCompletionSource<object>(), null) == null)
                {
                    _emptyQueueTask = _emptyQueueCompletionSource.Task;
                    _futureTaskWaitingCompletionSource.SetResult(null);
                    break;
                }

                // If pendingTasks is empty, but this.emptyQueueCompletionSource != null,
                // it is either Callback didn't updated this.emptyQueueCompletionSource yet, or another thread adding a task.
                // Wait a bit, then retry.
                spinWait.SpinOnce();
            }

            _pendingTasks.Enqueue(task);

            if (!_paused)
            {
                _syncContext.Post(_callback, null);
            }
        }

        [SecurityCritical]
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return SynchronizationContext.Current == _syncContext && TryExecuteTask(task);
        }

        [SecurityCritical]
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return null;
        }

        private void Callback()
        {
            if (_pendingTasks.IsEmpty)
            {
                return;
            }

            Task task;
            List<Exception> exceptions = new List<Exception>();

            while (_pendingTasks.TryPeek(out task))
            {
                if (task.IsCompleted)
                {
                    continue;
                }

                TryExecuteTask(task);
                if (task.IsFaulted && task.Exception != null)
                {
                    exceptions.AddRange(task.Exception.InnerExceptions);
                }

                // Dequeue task only when it is completed
                // Callback is called only in scheduler thread, so there will be no concurrency
                _pendingTasks.TryDequeue(out task);
            }

            Interlocked.Exchange(ref _futureTaskWaitingCompletionSource, new TaskCompletionSource<object>());
            TaskCompletionSource<object> tcs = Interlocked.Exchange(ref _emptyQueueCompletionSource, null);

            if (exceptions.Any())
            {
                tcs.SetException(exceptions);
            }
            else
            {
                tcs.SetResult(null);
            }
        }
    }
}
