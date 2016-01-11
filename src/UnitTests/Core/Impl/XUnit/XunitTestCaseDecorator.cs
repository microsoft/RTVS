using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UnitTests.Core.XUnit.MessageBusInjections;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit
{
    [ExcludeFromCodeCoverage]
    internal sealed class XunitTestCaseDecorator : XunitTestCaseDecoratorBase
    {
        public XunitTestCaseDecorator(IXunitTestCase testCase) : base(testCase)
        {
        }

        public XunitTestCaseDecorator() : base(null)
        {
        }

        protected override Task<RunSummary> RunAsyncOverride(IMessageSink diagnosticMessageSink, MessageBusOverride messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            messageBus.AddAfterStartingBeforeFinished(new VerifyGlobalProviderMessageBusInjection());
            return TestCase.RunAsync(diagnosticMessageSink, messageBus, constructorArguments, aggregator, cancellationTokenSource);
        }
    }
}
