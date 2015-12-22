using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Tests.Tokens;
using Microsoft.Languages.Core.Tests.Utility;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Markdown.Editor.Tests.Tokens {
    public class TokenizeStylesTest : TokenizeTestBase<MarkdownToken, MarkdownTokenType> {
        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Bold01() {
            var actualTokens = this.Tokenize(@"**bold** text **a**b**c**", new MdTokenizer());
            var expectedTokens = new TokenData<MarkdownTokenType>[]
            {
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Bold, 0, 8),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Bold, 14, 5),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Bold, 20, 5)
            };

            TokensCompare<MarkdownTokenType, MarkdownToken>.Compare(expectedTokens, actualTokens);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Bold02() {
            var actualTokens = this.Tokenize(@"**bold*", new MdTokenizer());
            Assert.Equal(0, actualTokens.Count);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Bold03() {
            var actualTokens = this.Tokenize(@"** bold**", new MdTokenizer());
            Assert.Equal(0, actualTokens.Count);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Italic01() {
            var actualTokens = this.Tokenize(@"*italic* text *a*b*c*", new MdTokenizer());
            var expectedTokens = new TokenData<MarkdownTokenType>[]
            {
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Italic, 0, 8),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Italic, 14, 3),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Italic, 18, 3)
            };

            TokensCompare<MarkdownTokenType, MarkdownToken>.Compare(expectedTokens, actualTokens);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Italic02() {
            var actualTokens = this.Tokenize(@"*italic", new MdTokenizer());
            Assert.Equal(0, actualTokens.Count);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Italic03() {
            var actualTokens = this.Tokenize(@"_ italic_", new MdTokenizer());
            Assert.Equal(0, actualTokens.Count);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Italic04() {
            var actualTokens = this.Tokenize(@"_italic_ text _a_b_c_", new MdTokenizer());
            var expectedTokens = new TokenData<MarkdownTokenType>[]
            {
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Italic, 0, 8),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Italic, 14, 3),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Italic, 18, 3)
            };

            TokensCompare<MarkdownTokenType, MarkdownToken>.Compare(expectedTokens, actualTokens);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Italic05() {
            var actualTokens = this.Tokenize(@"_italic", new MdTokenizer());
            Assert.Equal(0, actualTokens.Count);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Italic06() {
            var actualTokens = this.Tokenize(@"_ italic_", new MdTokenizer());
            Assert.Equal(0, actualTokens.Count);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Monospace01() {
            var actualTokens = this.Tokenize(@"`italic` text `a`b`c`", new MdTokenizer());
            var expectedTokens = new TokenData<MarkdownTokenType>[]
            {
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Monospace, 0, 8),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Monospace, 14, 3),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Monospace, 18, 3)
            };

            TokensCompare<MarkdownTokenType, MarkdownToken>.Compare(expectedTokens, actualTokens);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Monospace02() {
            var actualTokens = this.Tokenize(@"`italic", new MdTokenizer());
            Assert.Equal(0, actualTokens.Count);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Monospace03() {
            var actualTokens = this.Tokenize(@"` italic_", new MdTokenizer());
            Assert.Equal(0, actualTokens.Count);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Mixed01() {
            var actualTokens = this.Tokenize(@"**bold _text_ *a*b_c_**", new MdTokenizer());
            var expectedTokens = new TokenData<MarkdownTokenType>[]
            {
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Bold, 0, 7),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.BoldItalic, 7, 6),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Bold, 13, 1),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.BoldItalic, 14, 3),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Bold, 17, 1),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.BoldItalic, 18, 3),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Bold, 21, 2),
            };

            TokensCompare<MarkdownTokenType, MarkdownToken>.Compare(expectedTokens, actualTokens);
        }

        [Fact]
        [Trait("Md.Tokenizer", "")]
        public void TokenizeMd_Mixed02() {
            var actualTokens = this.Tokenize(@"_italic **text** **a**b**c**_", new MdTokenizer());
            var expectedTokens = new TokenData<MarkdownTokenType>[]
            {
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Italic, 0, 8),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.BoldItalic, 8, 8),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Italic, 16, 1),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.BoldItalic, 17, 5),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Italic, 22, 1),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.BoldItalic, 23, 5),
                new TokenData<MarkdownTokenType>(MarkdownTokenType.Italic, 28, 1),
            };

            TokensCompare<MarkdownTokenType, MarkdownToken>.Compare(expectedTokens, actualTokens);
        }
    }
}
