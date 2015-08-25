using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Languages.Core.Test.Text
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenStreamTest
    {
        enum TestTokenType
        {
            Token1,
            Token2,
            Token3,
            EndOfStream
        }

        [ExcludeFromCodeCoverage]
        class TestToken : Token<TestTokenType>
        {
            public TestToken(TestTokenType tokenType, ITextRange range):
                base(tokenType, range)
            {
            }
        }

        [TestMethod]
        public void EmptyTokenStreamTest()
        {
            var tokens = new TestToken[] { };
            var ts = CreateTokenStream(tokens);

            Assert.AreEqual(0, ts.Length);
            Assert.IsTrue(ts.IsEndOfStream());

            Assert.AreEqual(TestTokenType.EndOfStream, ts.CurrentToken.TokenType);
            Assert.AreEqual(TestTokenType.EndOfStream, ts.NextToken.TokenType);
            Assert.AreEqual(TestTokenType.EndOfStream, ts.PreviousToken.TokenType);
            Assert.AreEqual(0, ts.Position);

            var token = ts.Advance(10);
            Assert.AreEqual(token.TokenType, TestTokenType.EndOfStream);
            Assert.IsTrue(ts.IsEndOfStream());
            Assert.AreEqual(0, ts.Position);

            token = ts.Advance(-100);
            Assert.AreEqual(token.TokenType, TestTokenType.EndOfStream);
            Assert.AreEqual(-1, ts.Position);

            Assert.AreEqual(TestTokenType.EndOfStream, ts.CurrentToken.TokenType);
            Assert.AreEqual(TestTokenType.EndOfStream, ts.NextToken.TokenType);
            Assert.AreEqual(TestTokenType.EndOfStream, ts.PreviousToken.TokenType);

            ts.Reset();
            Assert.IsTrue(ts.IsEndOfStream());
            Assert.AreEqual(0, ts.Position);

            ts.MoveToNextToken();
            Assert.IsTrue(ts.IsEndOfStream());
            Assert.AreEqual(0, ts.Position);
        }

        [TestMethod]
        public void TokenStreamTest1()
        {
            var tokens = new TestToken[] 
            {
                new TestToken(TestTokenType.Token1, TextRange.FromBounds(0,1)),
                new TestToken(TestTokenType.Token2, TextRange.FromBounds(3,4)),
                new TestToken(TestTokenType.Token3, TextRange.FromBounds(6,8)),
                new TestToken(TestTokenType.Token1, TextRange.FromBounds(12,20)),
                new TestToken(TestTokenType.Token2, TextRange.FromBounds(20,21)),
            };

            var ts = CreateTokenStream(tokens);

            Assert.AreEqual(5, ts.Length);

            Assert.IsFalse(ts.IsEndOfStream());
            Assert.AreEqual(TestTokenType.Token1, ts.CurrentToken.TokenType);
            Assert.AreEqual(TestTokenType.Token2, ts.NextToken.TokenType);
            Assert.AreEqual(TestTokenType.EndOfStream, ts.PreviousToken.TokenType);
            Assert.AreEqual(0, ts.Position);

            ts.MoveToNextToken();

            Assert.IsFalse(ts.IsEndOfStream());
            Assert.AreEqual(TestTokenType.Token2, ts.CurrentToken.TokenType);
            Assert.AreEqual(TestTokenType.Token3, ts.NextToken.TokenType);
            Assert.AreEqual(TestTokenType.Token1, ts.PreviousToken.TokenType);
            Assert.AreEqual(TestTokenType.EndOfStream, ts.LookAhead(-2).TokenType);
            Assert.AreEqual(TestTokenType.EndOfStream, ts.LookAhead(100).TokenType);
            Assert.AreEqual(1, ts.Position);

            var token = ts.Advance(2);

            Assert.AreEqual(TestTokenType.Token1, ts.CurrentToken.TokenType);
            Assert.AreEqual(TestTokenType.Token2, ts.NextToken.TokenType);
            Assert.AreEqual(TestTokenType.Token3, ts.PreviousToken.TokenType);
            Assert.AreEqual(TestTokenType.Token2, ts.LookAhead(-2).TokenType);
            Assert.AreEqual(TestTokenType.EndOfStream, ts.LookAhead(100).TokenType);
            Assert.AreEqual(3, ts.Position);

            ts.MoveToNextToken();
            ts.MoveToNextToken();

            Assert.IsTrue(ts.IsEndOfStream());
            Assert.AreEqual(TestTokenType.EndOfStream, ts.CurrentToken.TokenType);
            Assert.AreEqual(TestTokenType.EndOfStream, ts.NextToken.TokenType);
            Assert.AreEqual(TestTokenType.Token2, ts.PreviousToken.TokenType);

            ts.MoveToNextToken();

            Assert.IsTrue(ts.IsEndOfStream());
            Assert.AreEqual(TestTokenType.EndOfStream, ts.CurrentToken.TokenType);
            Assert.AreEqual(TestTokenType.EndOfStream, ts.NextToken.TokenType);
            Assert.AreEqual(TestTokenType.Token2, ts.PreviousToken.TokenType);

            ts.Advance(-2);

            Assert.AreEqual(TestTokenType.Token1, ts.CurrentToken.TokenType);
            Assert.AreEqual(TestTokenType.Token2, ts.NextToken.TokenType);
            Assert.AreEqual(TestTokenType.Token3, ts.PreviousToken.TokenType);
            Assert.AreEqual(TestTokenType.Token2, ts.LookAhead(-2).TokenType);
            Assert.AreEqual(TestTokenType.EndOfStream, ts.LookAhead(100).TokenType);
            Assert.AreEqual(3, ts.Position);
        }

        [TestMethod]
        public void TokenStreamLineBreakTest()
        {
            var tokens = new TestToken[]
            {
                new TestToken(TestTokenType.Token1, TextRange.FromBounds(0,1)),
                new TestToken(TestTokenType.Token2, TextRange.FromBounds(3,4)),
                new TestToken(TestTokenType.Token3, TextRange.FromBounds(6,8)),
                new TestToken(TestTokenType.Token1, TextRange.FromBounds(12,20)),
                new TestToken(TestTokenType.Token2, TextRange.FromBounds(20,21)),
            };

            var ts = CreateTokenStream(tokens);
            ITextProvider textProvider = new TextStream("1  2  11  \r\n12345678x");

            Assert.IsFalse(ts.IsLineBreakAfter(textProvider, ts.Position));

            ts.Advance(2);
            Assert.IsTrue(ts.IsLineBreakAfter(textProvider, ts.Position));

            ts.Advance(-1);
            Assert.IsFalse(ts.IsLineBreakAfter(textProvider, ts.Position));

            ts.MoveToNextLine(textProvider);
            Assert.AreEqual(TestTokenType.Token1, ts.CurrentToken.TokenType);

            string s = textProvider.GetText(ts.CurrentToken);
            Assert.AreEqual("12345678", s);
        }

        [TestMethod]
        public void TokenStreamEnumerationTest()
        {
            var tokens = new TestToken[]
            {
                new TestToken(TestTokenType.Token1, TextRange.FromBounds(0,1)),
                new TestToken(TestTokenType.Token2, TextRange.FromBounds(3,4)),
                new TestToken(TestTokenType.Token3, TextRange.FromBounds(6,8)),
                new TestToken(TestTokenType.Token1, TextRange.FromBounds(12,20)),
                new TestToken(TestTokenType.Token2, TextRange.FromBounds(20,21)),
            };

            var ts = CreateTokenStream(tokens);
            int i = 0;
            foreach(var token in ts)
            {
                Assert.AreEqual(tokens[i], token);
                i++;
            }
        }

        private TokenStream<TestToken> CreateTokenStream(TestToken[] tokens)
        {
           return new TokenStream<TestToken>(
                new TextRangeCollection<TestToken>(tokens), 
                new TestToken(TestTokenType.EndOfStream, TextRange.EmptyRange));
        }
    }
}
