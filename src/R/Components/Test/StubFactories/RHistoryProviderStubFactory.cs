using Microsoft.R.Components.History;
using NSubstitute;

namespace Microsoft.R.Components.Test.StubFactories {
    public sealed class RHistoryProviderStubFactory {
        public static IRHistoryProvider CreateDefault() {
            return Substitute.For<IRHistoryProvider>();
        }
    }
}