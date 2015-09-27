using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Markdown.Editor.Test.Tokens
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeLinkTest : TokenizeTestBase<MdToken, MdTokenType>
    {
        [TestMethod]
        public void TokenizeMd_Link01()
        {
            var tokens = this.Tokenize(@"[text]()", new MdTokenizer());

            Assert.AreEqual(1, tokens.Count);

            Assert.AreEqual(MdTokenType.AltText, tokens[0].TokenType);
            Assert.AreEqual(0, tokens[0].Start);
            Assert.AreEqual(6, tokens[0].Length);
        }


        [TestMethod]
        public void TokenizeMd_Link02()
        {
            var tokens = this.Tokenize(@"[text] (", new MdTokenizer());
            Assert.AreEqual(0, tokens.Count);
        }

        [TestMethod]
        public void TokenizeMd_Link03()
        {
            var tokens = this.Tokenize(@"[text] ()", new MdTokenizer());
            Assert.AreEqual(0, tokens.Count);
        }
    }
}
