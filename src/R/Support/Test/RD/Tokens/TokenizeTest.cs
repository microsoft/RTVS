using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Support.RD.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Support.Test.RD.Tokens {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeRdTest : TokenizeTestBase<RdToken, RdTokenType> {
        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeRdKeywords1() {
            var tokens = this.Tokenize(@" \title", new RdTokenizer());

            Assert.AreEqual(1, tokens.Count);

            Assert.AreEqual(RdTokenType.Keyword, tokens[0].TokenType);
            Assert.AreEqual(1, tokens[0].Start);
            Assert.AreEqual(6, tokens[0].Length);
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeRdKeywords2() {
            var tokens = this.Tokenize(@" \title{}", new RdTokenizer());

            Assert.AreEqual(3, tokens.Count);

            Assert.AreEqual(RdTokenType.Keyword, tokens[0].TokenType);
            Assert.AreEqual(1, tokens[0].Start);
            Assert.AreEqual(6, tokens[0].Length);

            Assert.AreEqual(RdTokenType.OpenCurlyBrace, tokens[1].TokenType);
            Assert.AreEqual(7, tokens[1].Start);
            Assert.AreEqual(1, tokens[1].Length);

            Assert.AreEqual(RdTokenType.CloseCurlyBrace, tokens[2].TokenType);
            Assert.AreEqual(8, tokens[2].Start);
            Assert.AreEqual(1, tokens[2].Length);
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeRdPragmas1() {
            var tokens = this.Tokenize("#ifdef\ntext\n#endif", new RdTokenizer());

            Assert.AreEqual(2, tokens.Count);

            Assert.AreEqual(RdTokenType.Pragma, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(6, tokens[0].Length);

            Assert.AreEqual(RdTokenType.Pragma, tokens[1].TokenType);
            Assert.AreEqual(12, tokens[1].Start);
            Assert.AreEqual(6, tokens[1].Length);
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeRdPragmas2() {
            var tokens = this.Tokenize(" #if\ntext\n #endif", new RdTokenizer());

            Assert.AreEqual(0, tokens.Count);
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeRdArguments01() {
            var actualTokens = this.Tokenize(@"\a1{arg[text \a1[=a2]] text}", new RdTokenizer());
            var expectedTokens = new TokenData<RdTokenType>[]
            {
                new TokenData<RdTokenType>(RdTokenType.Keyword, 0, 3),
                new TokenData<RdTokenType>(RdTokenType.OpenCurlyBrace, 3, 1),
                new TokenData<RdTokenType>(RdTokenType.OpenSquareBracket, 7, 1),
                new TokenData<RdTokenType>(RdTokenType.Keyword, 13, 3),
                new TokenData<RdTokenType>(RdTokenType.OpenSquareBracket, 16, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseSquareBracket, 20, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseSquareBracket, 21, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseCurlyBrace, 27, 1),
            };

            TokensCompare<RdTokenType, RdToken>.Compare(expectedTokens, actualTokens);
        }


        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeRdArguments02() {
            var actualTokens = this.Tokenize(@"\method{as.matrix}{data.frame}(x)", new RdTokenizer());
            var expectedTokens = new TokenData<RdTokenType>[]
            {
                new TokenData<RdTokenType>(RdTokenType.Keyword, 0, 7),
                new TokenData<RdTokenType>(RdTokenType.OpenCurlyBrace, 7, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseCurlyBrace, 17, 1),
                new TokenData<RdTokenType>(RdTokenType.OpenCurlyBrace, 18, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseCurlyBrace, 29, 1),
            };

            TokensCompare<RdTokenType, RdToken>.Compare(expectedTokens, actualTokens);
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeRdArguments03() {
            var actualTokens = this.Tokenize(@"\usage{\method{as.matrix}{data.frame}(x)}", new RdTokenizer());
            var expectedTokens = new TokenData<RdTokenType>[]
            {
                new TokenData<RdTokenType>(RdTokenType.Keyword, 0, 6),
                new TokenData<RdTokenType>(RdTokenType.OpenCurlyBrace, 6, 1),
                new TokenData<RdTokenType>(RdTokenType.Keyword, 7, 7),
                new TokenData<RdTokenType>(RdTokenType.OpenCurlyBrace, 14, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseCurlyBrace, 24, 1),
                new TokenData<RdTokenType>(RdTokenType.OpenCurlyBrace, 25, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseCurlyBrace, 36, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseCurlyBrace, 40, 1),
            };

            TokensCompare<RdTokenType, RdToken>.Compare(expectedTokens, actualTokens);
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeRdArguments04() {
            var actualTokens = this.Tokenize(@"\ifelse{{latex}{\out[x]{~}}{ }}{}", new RdTokenizer());
            var expectedTokens = new TokenData<RdTokenType>[]
            {
                new TokenData<RdTokenType>(RdTokenType.Keyword, 0, 7),
                new TokenData<RdTokenType>(RdTokenType.OpenCurlyBrace, 7, 1),
                new TokenData<RdTokenType>(RdTokenType.OpenCurlyBrace, 8, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseCurlyBrace, 14, 1),
                new TokenData<RdTokenType>(RdTokenType.OpenCurlyBrace, 15, 1),
                new TokenData<RdTokenType>(RdTokenType.Keyword, 16, 4),
                new TokenData<RdTokenType>(RdTokenType.OpenSquareBracket, 20, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseSquareBracket, 22, 1),
                new TokenData<RdTokenType>(RdTokenType.OpenCurlyBrace, 23, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseCurlyBrace, 25, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseCurlyBrace, 26, 1),
                new TokenData<RdTokenType>(RdTokenType.OpenCurlyBrace, 27, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseCurlyBrace, 29, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseCurlyBrace, 30, 1),
                new TokenData<RdTokenType>(RdTokenType.OpenCurlyBrace, 31, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseCurlyBrace, 32, 1),
            };

            TokensCompare<RdTokenType, RdToken>.Compare(expectedTokens, actualTokens);
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeRdArguments05() {
            var actualTokens = this.Tokenize(@"\item{\dots}{ A }", new RdTokenizer());
            var expectedTokens = new TokenData<RdTokenType>[]
            {
                new TokenData<RdTokenType>(RdTokenType.Keyword, 0, 5),
                new TokenData<RdTokenType>(RdTokenType.OpenCurlyBrace, 5, 1),
                new TokenData<RdTokenType>(RdTokenType.Keyword, 6, 5),
                new TokenData<RdTokenType>(RdTokenType.CloseCurlyBrace, 11, 1),
                new TokenData<RdTokenType>(RdTokenType.OpenCurlyBrace, 12, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseCurlyBrace, 16, 1),
            };

            TokensCompare<RdTokenType, RdToken>.Compare(expectedTokens, actualTokens);
        }

        [TestMethod]
        [TestCategory("Rd.Tokenizer")]
        public void TokenizeRdVerbationContent() {
            var actualTokens = this.Tokenize(
@"\alias{\% \dots %foo}
#ifdef
%comment", new RdTokenizer());
            var expectedTokens = new TokenData<RdTokenType>[]
            {
                new TokenData<RdTokenType>(RdTokenType.Keyword, 0, 6),
                new TokenData<RdTokenType>(RdTokenType.OpenCurlyBrace, 6, 1),
                new TokenData<RdTokenType>(RdTokenType.Keyword, 10, 5),
                new TokenData<RdTokenType>(RdTokenType.CloseCurlyBrace, 20, 1),
                new TokenData<RdTokenType>(RdTokenType.Pragma, 23, 6),
                new TokenData<RdTokenType>(RdTokenType.Comment, 31, 8),
            };

            TokensCompare<RdTokenType, RdToken>.Compare(expectedTokens, actualTokens);
        }
    }
}
