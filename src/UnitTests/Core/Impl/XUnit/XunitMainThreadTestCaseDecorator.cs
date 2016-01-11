using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UnitTests.Core.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    internal sealed class XunitMainThreadTestCaseDecorator : XunitTestCaseDecoratorBase
    {
        public XunitMainThreadTestCaseDecorator() : this(null)
        {
        }

        public XunitMainThreadTestCaseDecorator(IXunitTestCase testCase) : base(testCase)
        {
        }

        protected override Task<RunSummary> RunAsyncOverride(IMessageSink diagnosticMessageSink, MessageBusOverride messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            return UIThreadHelper.Instance.Invoke(() => TestCase.RunAsync(diagnosticMessageSink, messageBus, constructorArguments, aggregator, cancellationTokenSource));
        }
    }
}