using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit.MessageBusInjections
{
    internal interface ITestCaseAfterStartingBeforeFinishedInjection
    {
        bool AfterStarting(IMessageBus messageBus, TestCaseStarting message);
        bool BeforeFinished(IMessageBus messageBus, TestCaseFinished message);
    }
}