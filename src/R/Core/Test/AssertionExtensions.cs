using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Assertions;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Test {
    [ExcludeFromCodeCoverage]
    internal static class AssertionExtensions {
        public static RTokenAssertions Should(this RToken token) {
            return new RTokenAssertions(token);
        }
    }
}