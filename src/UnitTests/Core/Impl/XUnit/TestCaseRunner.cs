// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    internal class TestCaseRunner : XunitTestCaseRunner {
        public TestCaseRunner(IXunitTestCase testCase, string displayName, string skipReason, object[] constructorArguments, object[] testMethodArguments, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
            : base(testCase, displayName, skipReason, constructorArguments, testMethodArguments, messageBus, aggregator, cancellationTokenSource) { }

        protected override Task<RunSummary> RunTestAsync() 
            => new XunitTestRunner(new XunitTest(TestCase, DisplayName), MessageBus, TestClass, ConstructorArguments, TestMethod, TestMethodArguments, SkipReason, BeforeAfterAttributes, new ExceptionAggregator(Aggregator), CancellationTokenSource).RunAsync();
    }
}