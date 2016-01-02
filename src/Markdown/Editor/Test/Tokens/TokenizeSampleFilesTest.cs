using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Tests.Tokens;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.R.Markdown.Editor.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Markdown.Editor.Test.Tokens {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeSampleMdFilesTest : TokenizeTestBase<MarkdownToken, MarkdownTokenType> {
        [TestMethod]
        [TestCategory("Md.Tokenizer")]
        public void TokenizeSampleMdFile01() {
            TokenizeFiles.TokenizeFile<MarkdownToken, MarkdownTokenType, MdTokenizer>(this.TestContext, @"Tokenization\01.md", "Markdown");
        }
    }
}
