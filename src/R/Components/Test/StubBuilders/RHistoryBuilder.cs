using Microsoft.R.Components.History;
using NSubstitute;

namespace Microsoft.R.Components.Test.StubBuilders {
    public sealed class RHistoryBuilder {
        public static IRHistory CreateDefault() {
            return Substitute.For<IRHistory>();
        }
    }
}