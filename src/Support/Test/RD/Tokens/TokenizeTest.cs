using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Support.RD.Tokens;
using Microsoft.R.Support.Test.RD.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Support.Test.Tokens
{
    [TestClass]
    public class TokenizeRdTest : TokenizeTestBase<RdToken, RdTokenType>
    {
        [TestMethod]
        public void TokenizeRdKeywords1()
        {
            var tokens = this.Tokenize(@" \title", new RdTokenizer());

            Assert.AreEqual(1, tokens.Count);

            Assert.AreEqual(RdTokenType.Keyword, tokens[0].TokenType);
            Assert.AreEqual(1, tokens[0].Start);
            Assert.AreEqual(6, tokens[0].Length);
        }

        [TestMethod]
        public void TokenizeRdKeywords2()
        {
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
        public void TokenizeRdPragmas1()
        {
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
        public void TokenizeRdPragmas2()
        {
            var tokens = this.Tokenize(" #if\ntext\n #endif", new RdTokenizer());

            Assert.AreEqual(0, tokens.Count);
        }

        [TestMethod]
        public void TokenizeRdArguments01()
        {
            var actualTokens = this.Tokenize(@"\a1{arg[text \a1[=a2]] text}", new RdTokenizer());
            var expectedTokens = new TokenData[]
            {
                new TokenData(RdTokenType.Keyword, 0, 3),
                new TokenData(RdTokenType.OpenCurlyBrace, 3, 1),
                new TokenData(RdTokenType.OpenSquareBracket, 7, 1),
                new TokenData(RdTokenType.Keyword, 13, 3),
                new TokenData(RdTokenType.OpenSquareBracket, 16, 1),
                new TokenData(RdTokenType.CloseSquareBracket, 20, 1),
                new TokenData(RdTokenType.CloseSquareBracket, 21, 1),
                new TokenData(RdTokenType.CloseCurlyBrace, 27, 1),
            };

            TokensCompare.Compare(expectedTokens, actualTokens);
        }


        [TestMethod]
        public void TokenizeRdArguments02()
        {
            var actualTokens = this.Tokenize(@"\method{as.matrix}{data.frame}(x)", new RdTokenizer());
            var expectedTokens = new TokenData[]
            {
                new TokenData(RdTokenType.Keyword, 0, 7),
                new TokenData(RdTokenType.OpenCurlyBrace, 7, 1),
                new TokenData(RdTokenType.CloseCurlyBrace, 17, 1),
                new TokenData(RdTokenType.OpenCurlyBrace, 18, 1),
                new TokenData(RdTokenType.CloseCurlyBrace, 29, 1),
            };

            TokensCompare.Compare(expectedTokens, actualTokens);
        }

        [TestMethod]
        public void TokenizeRdArguments03()
        {
            var actualTokens = this.Tokenize(@"\usage{\method{as.matrix}{data.frame}(x)}", new RdTokenizer());
            var expectedTokens = new TokenData[]
            {
                new TokenData(RdTokenType.Keyword, 0, 6),
                new TokenData(RdTokenType.OpenCurlyBrace, 6, 1),
                new TokenData(RdTokenType.Keyword, 7, 7),
                new TokenData(RdTokenType.OpenCurlyBrace, 14, 1),
                new TokenData(RdTokenType.CloseCurlyBrace, 24, 1),
                new TokenData(RdTokenType.OpenCurlyBrace, 25, 1),
                new TokenData(RdTokenType.CloseCurlyBrace, 36, 1),
                new TokenData(RdTokenType.CloseCurlyBrace, 40, 1),
            };

            TokensCompare.Compare(expectedTokens, actualTokens);
        }

        [TestMethod]
        public void TokenizeRdArguments04()
        {
            var actualTokens = this.Tokenize(@"\ifelse{{latex}{\out[x]{~}}{ }}{}", new RdTokenizer());
            var expectedTokens = new TokenData[]
            {
                new TokenData(RdTokenType.Keyword, 0, 7),
                new TokenData(RdTokenType.OpenCurlyBrace, 7, 1),
                new TokenData(RdTokenType.OpenCurlyBrace, 8, 1),
                new TokenData(RdTokenType.CloseCurlyBrace, 14, 1),
                new TokenData(RdTokenType.OpenCurlyBrace, 15, 1),
                new TokenData(RdTokenType.Keyword, 16, 4),
                new TokenData(RdTokenType.OpenSquareBracket, 20, 1),
                new TokenData(RdTokenType.CloseSquareBracket, 22, 1),
                new TokenData(RdTokenType.OpenCurlyBrace, 23, 1),
                new TokenData(RdTokenType.CloseCurlyBrace, 25, 1),
                new TokenData(RdTokenType.CloseCurlyBrace, 26, 1),
                new TokenData(RdTokenType.OpenCurlyBrace, 27, 1),
                new TokenData(RdTokenType.CloseCurlyBrace, 29, 1),
                new TokenData(RdTokenType.CloseCurlyBrace, 30, 1),
                new TokenData(RdTokenType.OpenCurlyBrace, 31, 1),
                new TokenData(RdTokenType.CloseCurlyBrace, 32, 1),
            };

            TokensCompare.Compare(expectedTokens, actualTokens);
        }

        //   
        [TestMethod]
        public void TokenizeRdArguments05()
        {
            var actualTokens = this.Tokenize(@"\item{\dots}{ A }", new RdTokenizer());
            var expectedTokens = new TokenData[]
            {
                new TokenData(RdTokenType.Keyword, 0, 5),
                new TokenData(RdTokenType.OpenCurlyBrace, 5, 1),
                new TokenData(RdTokenType.Keyword, 6, 5),
                new TokenData(RdTokenType.CloseCurlyBrace, 11, 1),
                new TokenData(RdTokenType.OpenCurlyBrace, 12, 1),
                new TokenData(RdTokenType.CloseCurlyBrace, 16, 1),
            };

            TokensCompare.Compare(expectedTokens, actualTokens);
        }
    }
}
