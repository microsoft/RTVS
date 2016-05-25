using System.Reflection;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit.MethodFixtures {
    public abstract class MethodFixtureBase : IMethodFixture {
        public static Task<Task<RunSummary>> CompletedInitializeTask { get; } = Task.FromResult(new Task<RunSummary>(() => null));

        public virtual Task<Task<RunSummary>> InitializeAsync(IXunitTestCase testCase, MethodInfo methodInfo, IMessageBus messageBus) {
            return CompletedInitializeTask;
        }

        public virtual Task DisposeAsync() {
            return Task.CompletedTask;
        }
    }
}
