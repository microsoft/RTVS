using Microsoft.R.Components.History;
using NSubstitute;

namespace Microsoft.R.Components.Test.StubFactories {
    public sealed class RHistoryStubFactory {
        public static IRHistory CreateDefault() {
            return Substitute.For<IRHistory>();
        }
    }
}