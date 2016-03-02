// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Common.Core {
    public static class TaskExtensions {
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
    }
}
