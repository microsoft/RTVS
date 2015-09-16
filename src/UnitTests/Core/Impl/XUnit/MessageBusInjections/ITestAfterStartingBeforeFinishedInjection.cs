using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit.MessageBusInjections
{
    internal interface ITestAfterStartingBeforeFinishedInjection
    {
        bool AfterStarting(IMessageBus messageBus, TestStarting message);
        bool BeforeFinished(IMessageBus messageBus, TestFinished message);
    }
}