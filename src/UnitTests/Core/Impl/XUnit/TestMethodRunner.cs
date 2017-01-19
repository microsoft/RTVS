// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    internal sealed class TestMethodRunner : XunitTestMethodRunner {
        private readonly IReadOnlyDictionary<Type, object> _assemblyFixtureMappings;
        private readonly IMessageSink _diagnosticMessageSink;
        private readonly object[] _constructorArguments;
        private readonly bool _hasMethodFixtures;

        public TestMethodRunner(ITestMethod testMethod, IReflectionTypeInfo @class, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, object[] constructorArguments, IReadOnlyDictionary<Type, object> assemblyFixtureMappings)
            : base(testMethod, @class, method, testCases, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource, constructorArguments) {
            _hasMethodFixtures = constructorArguments.OfType<IMethodFixture>().Any();
            _assemblyFixtureMappings = assemblyFixtureMappings;
            _diagnosticMessageSink = diagnosticMessageSink;
            _constructorArguments = constructorArguments;
        }

        protected override async Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase) {
            if (!_hasMethodFixtures) {
                return await base.RunTestCaseAsync(testCase);
            }

            var methodFixtureTypes = _constructorArguments
                .OfType<IMethodFixture>()
                .Select(f => f.GetType())
                .ToList();

            var methodFixtureFactories = _assemblyFixtureMappings.Values
                .OfType<IMethodFixtureFactory<IMethodFixture>>()
                .ToList();

            IDictionary<Type, IMethodFixture> methodFixtures;
            try {
                methodFixtures = MethodFixtureTypes.CreateMethodFixtures(methodFixtureTypes, methodFixtureFactories);
            } catch (Exception exception) {
                var runSummary = new RunSummary { Total = 1, Failed = 1, Time = 0 };
                MessageBus.QueueMessage(new TestFailed(new XunitTest(testCase, testCase.DisplayName), runSummary.Time, string.Empty, exception));
                return runSummary;
            }

            var testCaseConstructorArguments = GetConstructorArguments(methodFixtures);

            var testInput = new TestInput(testCase,
                Class.Type,
                TestMethod.Method.ToRuntimeMethod(),
                testCase.DisplayName,
                testCaseConstructorArguments,
                testCase.TestMethodArguments);

            var tasks = await InitializeMethodFixturesAsync(testInput, methodFixtures, MessageBus);
            var runTestTask = testCase.RunAsync(_diagnosticMessageSink, MessageBus, testCaseConstructorArguments, new ExceptionAggregator(Aggregator), CancellationTokenSource);
            tasks.Add(runTestTask);

            var runSummaryTask = await Task.WhenAny(tasks);
            await runSummaryTask;

            if (runSummaryTask != runTestTask) {
                CancellationTokenSource.Cancel();
            }

            await DisposeMethodFixturesAsync(runSummaryTask.Result, methodFixtures, MessageBus);
            return runSummaryTask.Result;
        }

        private object[] GetConstructorArguments(IDictionary<Type, IMethodFixture> methodFixtures) {
            var testCaseConstructorArguments = new object[_constructorArguments.Length];

            for (var i = 0; i < _constructorArguments.Length; i++) {
                var argument = _constructorArguments[i];
                IMethodFixture fixture;
                if (argument is IMethodFixture && methodFixtures.TryGetValue(argument.GetType(), out fixture)) {
                    testCaseConstructorArguments[i] = fixture;
                } else {
                    testCaseConstructorArguments[i] = _constructorArguments[i];
                }
            }

            return testCaseConstructorArguments;
        }

        public async Task<IList<Task<RunSummary>>> InitializeMethodFixturesAsync(ITestInput testInput, IDictionary<Type, IMethodFixture> methodFixtures, IMessageBus messageBus) {
            var tasks = new List<Task<RunSummary>>();
            foreach (var methodFixture in methodFixtures.Values) {
                var task = await methodFixture.InitializeAsync(testInput, messageBus);
                tasks.Add(task);
            }

            return tasks;
        }
        
        public async Task DisposeMethodFixturesAsync(RunSummary result, IDictionary<Type, IMethodFixture> methodFixtures, IMessageBus messageBus) {
            foreach (var methodFixture in methodFixtures.Values) {
                await methodFixture.DisposeAsync(result, messageBus);
            }
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