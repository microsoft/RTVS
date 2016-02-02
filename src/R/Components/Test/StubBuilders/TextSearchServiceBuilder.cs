using Microsoft.VisualStudio.Text.Operations;
using NSubstitute;

namespace Microsoft.R.Components.Test.StubBuilders {
    public sealed class TextSearchServiceBuilder {
        public static ITextSearchService2 CreateDefault() {
            return Substitute.For<ITextSearchService2>();
        }
    }
}