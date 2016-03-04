// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    public class TestMethodFixture : IDisposable {
        private readonly ConcurrentDictionary<Task, Lazy<IDisposable>> _observedTasks = new ConcurrentDictionary<Task, Lazy<IDisposable>>();
        private readonly TaskCompletionSource<object> _failedObservedTaskTcs;
        public MethodInfo MethodInfo { get; }
        internal Task FailedObservedTask => _failedObservedTaskTcs.Task;

        public TestMethodFixture() { }

        public TestMethodFixture(MethodInfo methodInfo) {
            MethodInfo = methodInfo;
            _failedObservedTaskTcs = new TaskCompletionSource<object>();
        }

        public IDisposable ObserveTaskFailure(Task task) {
            if (task == null) {
                throw new ArgumentNullException(nameof(task));
            }

            return _observedTasks.GetOrAdd(task, t => new Lazy<IDisposable>(() => StartObserving(t))).Value;
        }

        private IDisposable StartObserving(Task task) {
            task.ContinueWith(ContinueObservedTask, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            return Disposable.Create(() => UnobserveTaskFailure(task));
        }

        private void ContinueObservedTask(Task task) {
            Lazy<IDisposable> disposable;
            if (!_observedTasks.TryRemove(task, out disposable)) {
                return;
            }

            if (task.IsFaulted) {
                _failedObservedTaskTcs.TrySetException(task.Exception ?? new Exception());
            }
        }

        private void UnobserveTaskFailure(Task task) {
            Lazy<IDisposable> _;
            _observedTasks.TryRemove(task, out _);
        }

        public void Dispose() {
            _failedObservedTaskTcs.TrySetResult(null);
            _observedTasks.Clear();
        }
    }
}