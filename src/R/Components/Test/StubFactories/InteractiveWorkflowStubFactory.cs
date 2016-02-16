using Microsoft.R.Components.InteractiveWorkflow;
using NSubstitute;

namespace Microsoft.R.Components.Test.StubFactories {
    public sealed class InteractiveWorkflowStubFactory {
        public static IRInteractiveWorkflow CreateDefault() {
            return Substitute.For<IRInteractiveWorkflow>();
        }
    }
}
