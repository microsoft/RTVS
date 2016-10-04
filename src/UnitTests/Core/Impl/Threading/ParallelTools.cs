// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Collections;

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
            await Task.WhenAny(Task.WhenAll(tasks).SilenceException<Exception>(), Task.Delay(delayMs));
            return tasks.ToArray();
        }

        public static async Task<TResult[]> InvokeAsync<TResult>(int count, Func<int, TResult> method, Func<TResult, Task> taskSelector, int delayMs = 10000) {
            var results = Invoke(count, method).ToArray();
            var tasks = results.Select(taskSelector).ToArray();
            await Task.WhenAny(Task.WhenAll(tasks).SilenceException<Exception>(), Task.Delay(delayMs));
            return results;
        }

        public static async Task<Task[]> InvokeAsync(int count, Func<int, Task> method, int delayMs = 10000) {
            var results = Invoke(count, method);
            var tasks = results.ToArray();
            await Task.WhenAny(Task.WhenAll(tasks).SilenceException<Exception>(), Task.Delay(delayMs));
            return tasks.ToArray();
        }
    }
}
