using Microsoft.Common.Core.Tests.Utility;
using Microsoft.Languages.Core.Tests.Tokens;
using Microsoft.Languages.Editor.Tests.Utility;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Markdown.Editor.Tests.Tokens {
    public class TokenizeMdFiles : TokenizeTestBase<MarkdownToken, MarkdownTokenType> {
        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void Tokenize_MdFile01() {
            TokenizeFiles.TokenizeFile<MarkdownToken, MarkdownTokenType>(
                         @"Files\Tokenization\01.md",
                         "Markdown", typeof(MdTokenizer),
                         @"src\Markdown\Editor\Test\Files\Tokenization");
        }
    }
}
