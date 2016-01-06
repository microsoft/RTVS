using Microsoft.Languages.Core.Test.Assertions;
using Microsoft.Markdown.Editor.Tokens;

namespace Microsoft.Markdown.Editor.Test {
    internal static class AssertionExtensions {
        public static TokenAssertions<MarkdownTokenType> Should(this MarkdownToken token) {
            return new TokenAssertions<MarkdownTokenType>(token);
        }
    }
}