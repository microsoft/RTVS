// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public static async Task<T[]> InvokeAsync<T>(int count, Func<int, Task<T>> method) {
            var results = Invoke(count, method);

            IList<Task<T>> tasks = results.ToList();
            while (tasks.Count > 0) {
                await Task.WhenAny(tasks).Unwrap();
                tasks.RemoveWhere(t => t.Status == TaskStatus.RanToCompletion);
            }
            
            return results.Select(t => t.Result).ToArray();
        }

        public static async Task InvokeAsync(int count, Func<int, Task> method) {
            IList<Task> tasks = Invoke(count, method).ToList();
            while (tasks.Count > 0) {
                await Task.WhenAny(tasks).Unwrap();
                tasks.RemoveWhere(t => t.Status == TaskStatus.RanToCompletion);
            }
        }
    }
}
