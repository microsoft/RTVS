using Microsoft.VisualStudio.Text.Formatting;
using NSubstitute;

namespace Microsoft.R.Components.Test.StubBuilders {
    public sealed class RtfBuilderServiceBuilder {
        public static IRtfBuilderService CreateDefault() {
            return Substitute.For<IRtfBuilderService>();
        }
    }
}