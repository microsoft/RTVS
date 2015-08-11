using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Support.RD.Tokens;
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

            Assert.AreEqual(RdTokenType.OpenBrace, tokens[1].TokenType);
            Assert.AreEqual(7, tokens[1].Start);
            Assert.AreEqual(1, tokens[1].Length);

            Assert.AreEqual(RdTokenType.CloseBrace, tokens[2].TokenType);
            Assert.AreEqual(8, tokens[2].Start);
            Assert.AreEqual(1, tokens[2].Length);
        }

        [TestMethod]
        public void TokenizeRdPragmas1()
        {
            var tokens = this.Tokenize("#if\ntext\n#endif", new RdTokenizer());

            Assert.AreEqual(2, tokens.Count);

            Assert.AreEqual(RdTokenType.Pragma, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(3, tokens[0].Length);

            Assert.AreEqual(RdTokenType.Pragma, tokens[1].TokenType);
            Assert.AreEqual(9, tokens[1].Start);
            Assert.AreEqual(6, tokens[1].Length);
        }

        [TestMethod]
        public void TokenizeRdPragmas2()
        {
            var tokens = this.Tokenize(" #if\ntext\n #endif", new RdTokenizer());

            Assert.AreEqual(0, tokens.Count);
        }

        [TestMethod]
        public void TokenizeRdArguments()
        {
            var tokens = this.Tokenize(@"\a1{arg[text \a1[=a2]] text}", new RdTokenizer());

            Assert.AreEqual(9, tokens.Count);

            Assert.AreEqual(RdTokenType.Keyword, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(3, tokens[0].Length);

            Assert.AreEqual(RdTokenType.OpenBrace, tokens[1].TokenType);
            Assert.AreEqual(3, tokens[1].Start);
            Assert.AreEqual(1, tokens[1].Length);

            Assert.AreEqual(RdTokenType.Argument, tokens[2].TokenType);
            Assert.AreEqual(4, tokens[2].Start);
            Assert.AreEqual(9, tokens[2].Length);

            Assert.AreEqual(RdTokenType.Keyword, tokens[3].TokenType);
            Assert.AreEqual(13, tokens[3].Start);
            Assert.AreEqual(3, tokens[3].Length);

            Assert.AreEqual(RdTokenType.OpenBrace, tokens[4].TokenType);
            Assert.AreEqual(16, tokens[4].Start);
            Assert.AreEqual(1, tokens[4].Length);

            Assert.AreEqual(RdTokenType.Argument, tokens[5].TokenType);
            Assert.AreEqual(17, tokens[5].Start);
            Assert.AreEqual(3, tokens[5].Length);

            Assert.AreEqual(RdTokenType.CloseBrace, tokens[6].TokenType);
            Assert.AreEqual(20, tokens[6].Start);
            Assert.AreEqual(1, tokens[6].Length);

            Assert.AreEqual(RdTokenType.Argument, tokens[7].TokenType);
            Assert.AreEqual(21, tokens[7].Start);
            Assert.AreEqual(6, tokens[7].Length);

            Assert.AreEqual(RdTokenType.CloseBrace, tokens[8].TokenType);
            Assert.AreEqual(27, tokens[8].Start);
            Assert.AreEqual(1, tokens[8].Length);
        }
    }
}
