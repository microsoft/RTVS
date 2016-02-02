using Microsoft.R.Components.InteractiveWorkflow;
using NSubstitute;

namespace Microsoft.R.Components.Test.StubBuilders {
    public sealed class InteractiveWorkflowBuilder {
        public static IRInteractiveWorkflow CreateDefault() {
            return Substitute.For<IRInteractiveWorkflow>();
        }
    }
}
