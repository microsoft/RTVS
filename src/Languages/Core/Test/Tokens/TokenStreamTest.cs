using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Languages.Core.Tests.Text {
    public class TokenStreamTest {
        enum TestTokenType {
            Token1,
            Token2,
            Token3,
            EndOfStream
        }

        class TestToken : Token<TestTokenType> {
            public TestToken(TestTokenType tokenType, ITextRange range) :
                base(tokenType, range) {
            }
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void EmptyTokenStreamTest() {
            var tokens = new TestToken[] { };
            var ts = CreateTokenStream(tokens);

            Assert.Equal(0, ts.Length);
            Assert.True(ts.IsEndOfStream());

            Assert.Equal(TestTokenType.EndOfStream, ts.CurrentToken.TokenType);
            Assert.Equal(TestTokenType.EndOfStream, ts.NextToken.TokenType);
            Assert.Equal(TestTokenType.EndOfStream, ts.PreviousToken.TokenType);
            Assert.Equal(0, ts.Position);

            var token = ts.Advance(10);
            Assert.Equal(token.TokenType, TestTokenType.EndOfStream);
            Assert.True(ts.IsEndOfStream());
            Assert.Equal(0, ts.Position);

            token = ts.Advance(-100);
            Assert.Equal(token.TokenType, TestTokenType.EndOfStream);
            Assert.Equal(0, ts.Position);

            Assert.Equal(TestTokenType.EndOfStream, ts.CurrentToken.TokenType);
            Assert.Equal(TestTokenType.EndOfStream, ts.NextToken.TokenType);
            Assert.Equal(TestTokenType.EndOfStream, ts.PreviousToken.TokenType);

            ts.Position = 0;
            Assert.True(ts.IsEndOfStream());
            Assert.Equal(0, ts.Position);

            ts.MoveToNextToken();
            Assert.True(ts.IsEndOfStream());
            Assert.Equal(0, ts.Position);
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void TokenStreamTest1() {
            var tokens = new TestToken[]
            {
                new TestToken(TestTokenType.Token1, TextRange.FromBounds(0,1)),
                new TestToken(TestTokenType.Token2, TextRange.FromBounds(3,4)),
                new TestToken(TestTokenType.Token3, TextRange.FromBounds(6,8)),
                new TestToken(TestTokenType.Token1, TextRange.FromBounds(12,20)),
                new TestToken(TestTokenType.Token2, TextRange.FromBounds(20,21)),
            };

            var ts = CreateTokenStream(tokens);

            Assert.Equal(5, ts.Length);

            Assert.False(ts.IsEndOfStream());
            Assert.Equal(TestTokenType.Token1, ts.CurrentToken.TokenType);
            Assert.Equal(TestTokenType.Token2, ts.NextToken.TokenType);
            Assert.Equal(TestTokenType.EndOfStream, ts.PreviousToken.TokenType);
            Assert.Equal(0, ts.Position);

            ts.MoveToNextToken();

            Assert.False(ts.IsEndOfStream());
            Assert.Equal(TestTokenType.Token2, ts.CurrentToken.TokenType);
            Assert.Equal(TestTokenType.Token3, ts.NextToken.TokenType);
            Assert.Equal(TestTokenType.Token1, ts.PreviousToken.TokenType);
            Assert.Equal(TestTokenType.EndOfStream, ts.LookAhead(-2).TokenType);
            Assert.Equal(TestTokenType.EndOfStream, ts.LookAhead(100).TokenType);
            Assert.Equal(1, ts.Position);

            var token = ts.Advance(2);

            Assert.Equal(TestTokenType.Token1, ts.CurrentToken.TokenType);
            Assert.Equal(TestTokenType.Token2, ts.NextToken.TokenType);
            Assert.Equal(TestTokenType.Token3, ts.PreviousToken.TokenType);
            Assert.Equal(TestTokenType.Token2, ts.LookAhead(-2).TokenType);
            Assert.Equal(TestTokenType.EndOfStream, ts.LookAhead(100).TokenType);
            Assert.Equal(3, ts.Position);

            ts.MoveToNextToken();
            ts.MoveToNextToken();

            Assert.True(ts.IsEndOfStream());
            Assert.Equal(TestTokenType.EndOfStream, ts.CurrentToken.TokenType);
            Assert.Equal(TestTokenType.EndOfStream, ts.NextToken.TokenType);
            Assert.Equal(TestTokenType.Token2, ts.PreviousToken.TokenType);

            ts.MoveToNextToken();

            Assert.True(ts.IsEndOfStream());
            Assert.Equal(TestTokenType.EndOfStream, ts.CurrentToken.TokenType);
            Assert.Equal(TestTokenType.EndOfStream, ts.NextToken.TokenType);
            Assert.Equal(TestTokenType.Token2, ts.PreviousToken.TokenType);

            ts.Advance(-2);

            Assert.Equal(TestTokenType.Token1, ts.CurrentToken.TokenType);
            Assert.Equal(TestTokenType.Token2, ts.NextToken.TokenType);
            Assert.Equal(TestTokenType.Token3, ts.PreviousToken.TokenType);
            Assert.Equal(TestTokenType.Token2, ts.LookAhead(-2).TokenType);
            Assert.Equal(TestTokenType.EndOfStream, ts.LookAhead(100).TokenType);
            Assert.Equal(3, ts.Position);
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void TokenStreamLineBreakTest() {
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

            Assert.False(ts.IsLineBreakAfter(textProvider, ts.Position));

            ts.Advance(2);
            Assert.True(ts.IsLineBreakAfter(textProvider, ts.Position));

            ts.Advance(-1);
            Assert.False(ts.IsLineBreakAfter(textProvider, ts.Position));

            ts.MoveToNextLine(textProvider);
            Assert.Equal(TestTokenType.Token1, ts.CurrentToken.TokenType);

            string s = textProvider.GetText(ts.CurrentToken);
            Assert.Equal("12345678", s);
        }

        [Test]
        [Trait("Category", "Languages.Core")]
        public void TokenStreamEnumerationTest() {
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
            foreach (var token in ts) {
                Assert.Equal(tokens[i], token);
                i++;
            }
        }

        private TokenStream<TestToken> CreateTokenStream(TestToken[] tokens) {
            return new TokenStream<TestToken>(
                 new TextRangeCollection<TestToken>(tokens),
                 new TestToken(TestTokenType.EndOfStream, TextRange.EmptyRange));
        }
    }
}
