// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;

namespace Microsoft.UnitTests.Core.Threading {
    public static class ParallelTools {
        public static T[] Invoke<T>(int count, Func<int, T> method) {
            var results = new T[count];
            Parallel.For(0, count, i => results[i] = method(i));
            return results;
        }

        public static void Invoke(int count, Action<int> method) {
            Parallel.For(0, count, method);
        }

        public static async Task<Task<T>[]> InvokeAsync<T>(int count, Func<int, Task<T>> method, int delayMs = 10000) {
            var results = Invoke(count, method);
            var tasks = results.ToArray();
            await When(Task.WhenAll(tasks).SilenceException<Exception>(), delayMs);
            return tasks.ToArray();
        }

        public static async Task<TResult[]> InvokeAsync<TResult>(int count, Func<int, TResult> method, Func<TResult, Task> taskSelector, int delayMs = 10000) {
            var results = Invoke(count, method).ToArray();
            var tasks = results.Select(taskSelector).ToArray();
            await When(Task.WhenAll(tasks).SilenceException<Exception>(), delayMs);
            return results;
        }

        public static async Task<Task[]> InvokeAsync(int count, Func<int, Task> method, int delayMs = 10000) {
            var results = Invoke(count, method);
            var tasks = results.ToArray();
            await When(Task.WhenAll(tasks).SilenceException<Exception>(), delayMs);
            return tasks.ToArray();
        }

        public static Task WhenAll(params Task[] tasks) => WhenAll(10000, tasks);

        public static Task WhenAll(int timeout, params Task[] tasks) => WhenAll(timeout, null, tasks);

        public static async Task WhenAll(int timeout, string message, params Task[] tasks) {
            var timeoutTask = Task.Delay(timeout);
            await Task.WhenAny(timeoutTask, Task.WhenAll(tasks));
            if (timeoutTask.IsCompleted) {
                var indexes = tasks.IndexWhere(t => !t.IsCompleted).ToList();
                if (indexes.Any()) {
                    var notCompletedReason = indexes.Count == 1
                        ? $"the task at {indexes[0]} is still not completed"
                        : $"the tasks at {string.Join(", ", indexes)} are still not completed";
                    message = message ?? $"{nameof(WhenAll)} failed by timeout, {notCompletedReason}";
                    throw new TimeoutException(message);
                }
            }
        }

        public static Task When(Task task, string message = null) => When(task, 10000, message);

        public static async Task When(Task task, int timeout, string message = null) {
            var timeoutTask = Task.Delay(timeout);
            await Task.WhenAny(timeoutTask, task);
            if (timeoutTask.IsCompleted) {
                throw new TimeoutException(message ?? "Test failed by timeout, task is still not completed");
            }
        }
    }
}
