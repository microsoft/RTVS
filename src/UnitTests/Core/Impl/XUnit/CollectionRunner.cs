using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    internal sealed class CollectionRunner : XunitTestCollectionRunner {
        private readonly IReadOnlyDictionary<Type, object> _assemblyFixtureMappings;
        private readonly IMessageSink _diagnosticMessageSink;

        public CollectionRunner(ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ITestCaseOrderer testCaseOrderer, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, IReadOnlyDictionary<Type, object> assemblyFixtureMappings)
            : base(testCollection, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource) {
            _diagnosticMessageSink = diagnosticMessageSink;
            _assemblyFixtureMappings = assemblyFixtureMappings;
        }

        protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo typeInfo, IEnumerable<IXunitTestCase> testCases) {
            return new ClassRunner(testClass, typeInfo, testCases, _diagnosticMessageSink, MessageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), CancellationTokenSource, CollectionFixtureMappings, _assemblyFixtureMappings).RunAsync();
        }
    }
}