using Microsoft.Languages.Core.Test.Assertions;
using Microsoft.R.Support.RD.Tokens;

namespace Microsoft.R.Support.Test {
    internal static class AssertionExtensions {
        public static TokenAssertions<RdTokenType> Should(this RdToken token) {
            return new TokenAssertions<RdTokenType>(token);
        }
    }
}