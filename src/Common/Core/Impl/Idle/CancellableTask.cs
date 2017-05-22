// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Idle {
    // NOTE: task does not provide any facilities to wait for its completion.
    // The reason is that in WPF app one can't wait for an object on a main
    // thread since if object is also set on a main thread the app will hang.
    // This is not true in native code or COM since RPC will reenter main
    // thread context and make the call that will set the event. WPF disptcher
    // does not have this capability. Thus if you need to wait for background
    // processing to complete, make sure event is set from a background 
    // thread and not in a main thread callback. Also, consider pulling data
    // from background thread directly rather than making Dispatcher.Invoke call
    // that will try transitioning data to the main thread.
    public abstract class CancellableTask : IDisposable {
        // TODO: clean old code, use cancellation token, etc
        private Task _task;
        private readonly ManualResetEventSlim _taskCompleted = new ManualResetEventSlim(true);
        private long _canceled;
        private long _taskId = 1;

        /// <summary>
        /// Task id helps tracking which task is completed or which is being canceled. 
        /// Consider situation when caller cancels task and starts a new one.
        /// For example, parser may be running while user changed text buffer since
        /// parsing started. So AST builer cancels running (or scheduled) task
        /// and starts a new one. It may be a while before canceled task actually completes
        /// since it depends when (and if) task is going to check for cancellation.
        /// Thus one may receive an event that task completed from a task that got 
        /// canceled or managed to complete, but caller since spawned another task.
        /// </summary>
        protected long TaskId => Interlocked.Read(ref _taskId);

        /// <summary>
        /// Launches the task synchronously
        /// </summary>
        /// <param name="runAction">Action to invoke on a background thread.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void RunSynchronously(Action<Func<bool>> runAction) => Run(runAction, false);

        /// <summary>
        /// Launches the task. 
        /// </summary>
        /// <param name="runAction">Action to invoke on a background thread.</param>
        /// <param name="async">True if processing should happen asynchronously, false otherwise.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual void Run(Action<Func<bool>> runAction, bool async) {
            // New task, increment id;
            Interlocked.Increment(ref _taskId);

            // Do not run multiple tasks. According to http://msdn.microsoft.com/en-us/library/dd270681.aspx
            // task can only be disposed if it is RanToCompletion, Faulted, or Canceled.
            // If task is not in one of those states, we queue it for disposal later.

            // Note that task may actually still be running since it may not be able to
            // cancel immediately. The important part is that we don't care about its results.
            // This means that task we no longer care about can still signal completion and 
            // we should ignore it. We use taskId member to determine if task calling is 
            // the current task.

            SignalTaskBegins();

            _task = new Task(taskId => {
                try {
                    runAction(() => IsCancellationRequested() || TaskId != (long)taskId);
                } catch (Exception ex) when (!ex.IsCriticalException()) { }
            }, TaskId);

            if (!async) {
                _task.RunSynchronously();
            } else {
                _task.Start();
            }
        }

        /// <summary>
        /// Provides a basic way to wait for the task completion. One has to be very careful when 
        /// calling this method on an application UI thread since call may hang depending on the
        /// task nature. It is recommended that derived class implements appropriate wait
        /// technique depending how task body actually signals completion. Default implementation
        /// 
        /// </summary>
        public virtual void WaitForCompletion(int milliseconds) {
            // TPL has 8 (eight!) task states so we must be careful to wait
            // only when task is actually running or is about to be ran.
            if (_task != null && _taskCompleted != null && IsTaskRunning()) {
                _taskCompleted.Wait(milliseconds);
            }
        }

        /// <summary>
        /// Checks if main thread requested task cancellation
        /// </summary>
        public bool IsCancellationRequested() => Interlocked.Read(ref _canceled) > 0;

        /// <summary>
        /// Attempts to cancel the task by calling Cancel() on cancellation token source.
        /// Cancellation is cooperative and task may continue running until it reaches
        /// point where it checks for cancellation and then eventually terminate.
        /// </summary>
        public virtual void Cancel() {
            if (_task != null && _taskCompleted != null) {
                Interlocked.Exchange(ref _canceled, 1);
                Interlocked.Increment(ref _taskId);
                _taskCompleted.Set();
            }
        }

        private void SignalTaskBegins() {
            if (_taskCompleted != null) {
                Interlocked.Exchange(ref _canceled, 0);
                _taskCompleted.Reset();
            }
        }

        protected void SignalTaskComplete(long taskId) {
            if (TaskId == taskId && _taskCompleted != null) {
                _taskCompleted.Set();
                Interlocked.Exchange(ref _canceled, 0);
            }
        }

        /// <summary>
        /// Tell if task has finished. 'Completed' does not necessarily
        /// mean task completed successfully. It could as well be canceled.
        /// </summary>
        /// <returns></returns>
        public bool IsTaskCompleted() => _taskCompleted == null || _taskCompleted.IsSet;

        public bool IsTaskRunning() => !IsTaskCompleted();

        #region Dispose
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_task")]
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                Cancel();
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
