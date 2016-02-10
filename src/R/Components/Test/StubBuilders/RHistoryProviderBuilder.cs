using Microsoft.R.Components.History;
using NSubstitute;

namespace Microsoft.R.Components.Test.StubBuilders {
    public sealed class RHistoryProviderBuilder {
        public static IRHistoryProvider CreateDefault() {
            return Substitute.For<IRHistoryProvider>();
        }
    }
}