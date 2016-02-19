using Microsoft.VisualStudio.Text.Operations;
using NSubstitute;

namespace Microsoft.R.Components.Test.StubFactories {
    public sealed class TextSearchServiceStubFactory {
        public static ITextSearchService2 CreateDefault() {
            return Substitute.For<ITextSearchService2>();
        }
    }
}