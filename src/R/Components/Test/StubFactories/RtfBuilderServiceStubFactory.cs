using Microsoft.VisualStudio.Text.Formatting;
using NSubstitute;

namespace Microsoft.R.Components.Test.StubFactories {
    public sealed class RtfBuilderServiceStubFactory {
        public static IRtfBuilderService CreateDefault() {
            return Substitute.For<IRtfBuilderService>();
        }
    }
}