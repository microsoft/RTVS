// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UnitTests.Core.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    internal sealed class TestMethodRunner : XunitTestMethodRunner {
        private readonly IReadOnlyDictionary<Type, object> _assemblyFixtureMappings;
        private readonly XunitTestEnvironment _testEnvironment;
        private readonly IMessageSink _diagnosticMessageSink;
        private readonly object[] _constructorArguments;
        private readonly Stopwatch _stopwatch;

        public TestMethodRunner(ITestMethod testMethod, IReflectionTypeInfo @class, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, object[] constructorArguments, IReadOnlyDictionary<Type, object> assemblyFixtureMappings, XunitTestEnvironment testEnvironment)
            : base(testMethod, @class, method, testCases, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource, constructorArguments) {
            _assemblyFixtureMappings = assemblyFixtureMappings;
            _testEnvironment = testEnvironment;
            _diagnosticMessageSink = diagnosticMessageSink;
            _constructorArguments = constructorArguments;
            _stopwatch = new Stopwatch();
        }

        protected override async Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase) {
            using (var testMainThread = UIThreadHelper.Instance.CreateTestMainThread())
            using (var taskObserver = _testEnvironment.UseTaskObserver()) {
                if (_constructorArguments.OfType<IMethodFixture>().Any()) {
                    return await RunTestCaseWithMethodFixturesAsync(testCase, taskObserver, testMainThread);
                }

                var testCaseRunSummay = await GetTestRunSummary(base.RunTestCaseAsync(testCase), taskObserver.Task);
                await WaitForObservedTasksAsync(testCase, testCaseRunSummay, taskObserver, testMainThread);
                return testCaseRunSummay;
            }
        }

        private async Task<RunSummary> RunTestCaseWithMethodFixturesAsync(IXunitTestCase testCase, TaskObserver taskObserver, TestMainThread testMainThread) {
            var runSummary = new RunSummary();
            var methodFixtures = CreateMethodFixtures(testCase, runSummary);

            if (Aggregator.HasExceptions) {
                return runSummary;
            }

            var testCaseConstructorArguments = await InitializeMethodFixturesAsync(testCase, runSummary, methodFixtures);

            if (!Aggregator.HasExceptions) {
                var testCaseRunTask = testCase.RunAsync(_diagnosticMessageSink, MessageBus, testCaseConstructorArguments, new ExceptionAggregator(Aggregator), CancellationTokenSource);
                var testCaseRunSummary = await GetTestRunSummary(testCaseRunTask, taskObserver.Task);
                runSummary.Aggregate(testCaseRunSummary);
            }

            await DisposeMethodFixturesAsync(testCase, runSummary, methodFixtures);
            await WaitForObservedTasksAsync(testCase, runSummary, taskObserver, testMainThread);

            return runSummary;
        }

        private IDictionary<Type, object> CreateMethodFixtures(IXunitTestCase testCase, RunSummary runSummary) {
            var methodFixtureTypes = _constructorArguments
                .OfType<IMethodFixture>()
                .Select(f => f.GetType())
                .ToList();

            var methodFixtureFactories = _assemblyFixtureMappings.Values
                .OfType<IMethodFixtureFactory<object>>()
                .ToList();

            try {
                _stopwatch.Restart();
                var methodFixtures = MethodFixtureTypes.CreateMethodFixtures(methodFixtureTypes, methodFixtureFactories);
                _stopwatch.Stop();
                runSummary.Aggregate(new RunSummary { Time = (decimal)_stopwatch.Elapsed.TotalSeconds });
                return methodFixtures;
            } catch (Exception exception) {
                _stopwatch.Stop();
                runSummary.Aggregate(RegisterFailedRunSummary(testCase, (decimal)_stopwatch.Elapsed.TotalSeconds, exception));
                return null;
            }
        }

        private async Task<object[]> InitializeMethodFixturesAsync(IXunitTestCase testCase, RunSummary runSummary, IDictionary<Type, object> methodFixtures) {
            var constructorArguments = GetConstructorArguments(methodFixtures);
            var testInput = CreateTestInput(testCase, constructorArguments);

            foreach (var methodFixture in methodFixtures.Values.OfType<IMethodFixture>().Distinct()) {
                await RunAsync(testCase, () => methodFixture.InitializeAsync(testInput, MessageBus), runSummary, $"Method fixture {methodFixture.GetType()} needs too much time to initialize");
            }

            return constructorArguments;
        }

        private async Task DisposeMethodFixturesAsync(IXunitTestCase testCase, RunSummary runSummary, IDictionary<Type, object> methodFixtures) {
            foreach (var methodFixture in methodFixtures.Values.OfType<IMethodFixture>().Distinct()) {
                await RunAsync(testCase, () => methodFixture.DisposeAsync(runSummary, MessageBus), runSummary, $"Method fixture {methodFixture.GetType()} needs too much time to dispose");
            }
        }

        private Task WaitForObservedTasksAsync(IXunitTestCase testCase, RunSummary runSummary, TaskObserver taskObserver, TestMainThread testMainThread) {
            testMainThread.CancelPendingTasks();
            taskObserver.TestCompleted();
            return RunAsync(testCase, () => taskObserver.Task, runSummary, "Tasks that have been started during test run are still not completed");
        }

        private ITestInput CreateTestInput(IXunitTestCase testCase, object[] testCaseConstructorArguments) {
            return new TestInput(testCase,
                Class.Type,
                TestMethod.Method.ToRuntimeMethod(),
                testCase.DisplayName,
                testCaseConstructorArguments,
                testCase.TestMethodArguments);
        }

        private object[] GetConstructorArguments(IDictionary<Type, object> methodFixtures) {
            var testCaseConstructorArguments = new object[_constructorArguments.Length];

            for (var i = 0; i < _constructorArguments.Length; i++) {
                var argument = _constructorArguments[i];
                if (methodFixtures.TryGetValue(argument.GetType(), out object fixture)) {
                    testCaseConstructorArguments[i] = fixture;
                } else {
                    testCaseConstructorArguments[i] = _constructorArguments[i];
                }
            }

            return testCaseConstructorArguments;
        }
        
        private async Task<RunSummary> GetTestRunSummary(Task<RunSummary> testCaseRunTask, Task<Exception> taskObserverTask) {
            await Task.WhenAny(testCaseRunTask, taskObserverTask);
            if (testCaseRunTask.IsCompleted) {
                return testCaseRunTask.Result;
            }

            CancellationTokenSource.Cancel();
            var testCaseSummary = await testCaseRunTask;

            if (taskObserverTask.IsFaulted) {
                Aggregator.Add(taskObserverTask.Exception);
                testCaseSummary.Failed = 1;
            }

            return testCaseSummary;
        }

        private async Task RunAsync(IXunitTestCase testCase, Func<Task> action, RunSummary runSummary, string timeoutMessage) {
            Exception exception = null;
            _stopwatch.Restart();
            try {
                var task = action();
                await ParallelTools.When(task, 60_000, timeoutMessage);
                await task;
            } catch (Exception ex) {
                exception = ex;    
            }
            _stopwatch.Stop();

            var time = (decimal) _stopwatch.Elapsed.TotalSeconds;
            var taskRunSummary = exception != null
                ? RegisterFailedRunSummary(testCase, time, exception)
                : new RunSummary {Time = time};

            runSummary.Aggregate(taskRunSummary);
        }

        private RunSummary RegisterFailedRunSummary(IXunitTestCase testCase, decimal time, Exception exception) {
            Aggregator.Add(exception);
            var caseSummary = new RunSummary {Total = 1, Failed = 1, Time = time};
            MessageBus.QueueMessage(new TestFailed(new XunitTest(testCase, testCase.DisplayName), caseSummary.Time, string.Empty, exception));
            return caseSummary;
        }

        private class TestInput : ITestInput {
            public IXunitTestCase TestCase { get; }
            public Type TestClass { get; }
            public MethodInfo TestMethod { get; }
            public string DisplayName { get; }
            public string FileSytemSafeName { get; }
            public IReadOnlyList<object> ConstructorArguments { get; }
            public IReadOnlyList<object> TestMethodArguments { get; }

            public TestInput(IXunitTestCase testCase, Type testClass, MethodInfo testMethod, string displayName, object[] constructorArguments, object[] testMethodArguments) {
                TestCase = testCase;
                TestClass = testClass;
                TestMethod = testMethod;
                DisplayName = displayName;
                FileSytemSafeName = $"{GetFileSystemSafeName(testClass)}_{testMethod.Name}";
                ConstructorArguments = new ReadOnlyCollection<object>(constructorArguments);
                TestMethodArguments = new ReadOnlyCollection<object>(testMethodArguments ?? new object[0]);
            }

            private static string GetFileSystemSafeName(Type type) {
                var sb = new StringBuilder(type.ToString())
                    .Replace('+', '-')
                    .Replace('#', '-');

                for (var i = sb.Length - 1; i >= 0; i--) {
                    if (sb[i] == '`') {
                        sb.Remove(i, 1);
                    }
                }
             
                return sb.ToString();
            }
        }
    }
}