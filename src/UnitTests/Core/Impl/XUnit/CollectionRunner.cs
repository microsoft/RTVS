// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    internal sealed class CollectionRunner : XunitTestCollectionRunner {
        private readonly IReadOnlyDictionary<Type, object> _assemblyFixtureMappings;
        private readonly XunitTestEnvironment _testEnvironment;
        private readonly IMessageSink _diagnosticMessageSink;

        public CollectionRunner(ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ITestCaseOrderer testCaseOrderer, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, IReadOnlyDictionary<Type, object> assemblyFixtureMappings, XunitTestEnvironment testEnvironment)
            : base(testCollection, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource) {
            _diagnosticMessageSink = diagnosticMessageSink;
            _assemblyFixtureMappings = assemblyFixtureMappings;
            _testEnvironment = testEnvironment;
        }

        protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo typeInfo, IEnumerable<IXunitTestCase> testCases) {
            return new ClassRunner(testClass, typeInfo, testCases, _diagnosticMessageSink, MessageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), CancellationTokenSource, CollectionFixtureMappings, _assemblyFixtureMappings, _testEnvironment).RunAsync();
        }
    }
}