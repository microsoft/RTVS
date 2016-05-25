// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    internal sealed class TestCaseRunnerWithMethodFixtures : TestCaseRunner {
        private readonly IList<IMethodFixture> _methodFixtures = new List<IMethodFixture>();

        public TestCaseRunnerWithMethodFixtures(IXunitTestCase testCase, string displayName, string skipReason, object[] constructorArguments, object[] testMethodArguments, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
            : base(testCase, displayName, skipReason, constructorArguments, testMethodArguments, messageBus, aggregator, cancellationTokenSource) {

            for (var i = 0; i < constructorArguments.Length; i++) {
                var methodFixture = constructorArguments[i] as IMethodFixture;
                if (methodFixture == null) {
                    continue;
                }

                var methodFixtureType = methodFixture.GetType();
                methodFixture = (IMethodFixture)Activator.CreateInstance(methodFixtureType);
                constructorArguments[i] = methodFixture;
                _methodFixtures.Add(methodFixture);
            }
        }

        protected override async Task<RunSummary> RunTestAsync() {
            var tasks = new List<Task<RunSummary>>();

            foreach (var methodFixture in _methodFixtures) {
                var task = await methodFixture.InitializeAsync(TestCase, TestMethod, MessageBus);
                tasks.Add(task);
            }

            var runTestTask = base.RunTestAsync();
            tasks.Add(runTestTask);

            var runSummaryTask = await Task.WhenAny(tasks);
            await runSummaryTask;

            if (runSummaryTask != runTestTask) {
                CancellationTokenSource.Cancel();
            }

            foreach (var methodFixture in _methodFixtures) {
                await methodFixture.DisposeAsync();
            }

            return runSummaryTask.Result;
        }
    }
}