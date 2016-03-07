// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core {
    public static class TaskExtensions {
        public static Task FailOnTimeout(this Task task, int millisecondsTimeout) {
            return task.TimeoutAfterImpl<object>(millisecondsTimeout);
        }

        public static Task<T> FailOnTimeout<T>(this Task task, int millisecondsTimeout) {
            return (Task<T>) task.TimeoutAfterImpl<T>(millisecondsTimeout);
        }

        public static Task TimeoutAfterImpl<T>(this Task task, int millisecondsTimeout) {
            if (task.IsCompleted || (millisecondsTimeout == Timeout.Infinite)) {
                return task;
            }

            if (millisecondsTimeout == 0) {
                return Task.FromException<T>(new TimeoutException());
            }

            var tcs = new TaskCompletionSource<T>();
            var cancelByTimeout = new TimerCallback(state => ((TaskCompletionSource<T>)state).TrySetException(new TimeoutException()));
            var timer = new Timer(cancelByTimeout, tcs, millisecondsTimeout, Timeout.Infinite);
            var taskState = new TimeoutAfterState<T>(timer, tcs);

            var continuation = new Action<Task, object>((source, state) => {
                var timeoutAfterState = (TimeoutAfterState<T>)state;
                timeoutAfterState.Timer.Dispose();

                switch (source.Status) {
                    case TaskStatus.Faulted:
                        timeoutAfterState.Tcs.TrySetException(source.Exception);
                        break;
                    case TaskStatus.Canceled:
                        timeoutAfterState.Tcs.TrySetCanceled();
                        break;
                    case TaskStatus.RanToCompletion:
                        var typedTask = source as Task<T>;
                        timeoutAfterState.Tcs.TrySetResult(typedTask != null ? typedTask.Result : default(T));
                        break;
                }
            });

            task.ContinueWith(continuation, taskState, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            return tcs.Task;
        }
        
        public static Task ContinueOnRanToCompletion<TResult>(this Task<TResult> task, Action<TResult> action) {
            return task.ContinueWith(t => action(t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// Suppresses warnings about unawaited tasks and ensures that unhandled
        /// errors will cause the process to terminate.
        /// </summary>
        /// <remarks>
        /// <see cref="OperationCanceledException"/> is always ignored.
        /// </remarks>
        public static async void DoNotWait(this Task task) {
            await task.SilenceException<OperationCanceledException>();
        }

        /// <summary>
        /// Waits for a task to complete. If an exception occurs, the exception
        /// will be raised without being wrapped in a
        /// <see cref="AggregateException"/>.
        /// </summary>
        public static void WaitAndUnwrapExceptions(this Task task) {
            task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Waits for a task to complete. If an exception occurs, the exception
        /// will be raised without being wrapped in a
        /// <see cref="AggregateException"/>.
        /// </summary>
        public static T WaitAndUnwrapExceptions<T>(this Task<T> task) {
            return task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Silently handles the specified exception.
        /// </summary>
        public static Task SilenceException<T>(this Task task) where T : Exception {
            var tcs = new TaskCompletionSource<object>();
            task.ContinueWith(t => {
                try {
                    t.Wait();
                    tcs.SetResult(null);
                } catch (AggregateException ex) {
                    try {
                        ex.Handle(e => e is T);
                        tcs.SetResult(null);
                    } catch (AggregateException ex2) {
                        tcs.SetException(ex2.InnerExceptions);
                    }
                } catch (OperationCanceledException) {
                    tcs.SetCanceled();
                }
            });
            return tcs.Task;
        }

        public class TimeoutAfterState<T> {
            public Timer Timer { get; }
            public TaskCompletionSource<T> Tcs { get; }

            public TimeoutAfterState(Timer timer, TaskCompletionSource<T> tcs) {
                Timer = timer;
                Tcs = tcs;
            }
        }
    }
}
