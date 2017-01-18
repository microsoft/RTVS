// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit.MethodFixtures {
    public class TaskObserverMethodFixture : IMethodFixture {
        private ConcurrentDictionary<Task, Lazy<IDisposable>> _observedTasks;
        private TaskCompletionSource<RunSummary> _runSummaryTcs;
        private Stopwatch _stopwatch;
        private IXunitTestCase _testCase;
        private IMessageBus _messageBus;

        public Task<Task<RunSummary>> InitializeAsync(ITestInput testInput, IMessageBus messageBus) {
            _runSummaryTcs = new TaskCompletionSource<RunSummary>();
            _stopwatch = Stopwatch.StartNew();
            _testCase = testInput.TestCase;
            _messageBus = messageBus;
            _observedTasks = new ConcurrentDictionary<Task, Lazy<IDisposable>>();

            return Task.FromResult(_runSummaryTcs.Task);
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

            if (!task.IsFaulted) {
                return;
            }

            var runSummary = new RunSummary { Total = 1, Failed = 1, Time = (decimal)_stopwatch.Elapsed.TotalSeconds };
            _messageBus.QueueMessage(new TestFailed(new XunitTest(_testCase, _testCase.DisplayName), runSummary.Time, string.Empty, task.Exception));
            _runSummaryTcs.SetResult(runSummary);
        }

        private void UnobserveTaskFailure(Task task) {
            Lazy<IDisposable> _;
            _observedTasks.TryRemove(task, out _);
        }
        
        public Task DisposeAsync(RunSummary result, IMessageBus messageBus) {
            _stopwatch.Stop();
            _observedTasks.Clear();
            return Task.CompletedTask;
        }
    }
}
