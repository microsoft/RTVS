// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Languages.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class TokenStreamTest {
        enum TestTokenType {
            Token1,
            Token2,
            Token3,
            EndOfStream
        }

        [ExcludeFromCodeCoverage]
        class TestToken : Token<TestTokenType> {
            public TestToken(TestTokenType tokenType, ITextRange range) :
                base(tokenType, range) {
            }
        }

        [Test]
        [Category.Languages.Core]
        public void EmptyTokenStreamTest() {
            var tokens = new TestToken[] { };
            var ts = CreateTokenStream(tokens);

            ts.Length.Should().Be(0);
            ts.IsEndOfStream().Should().BeTrue();

            ts.CurrentToken.Should().HaveType(TestTokenType.EndOfStream);
            ts.NextToken.Should().HaveType(TestTokenType.EndOfStream);
            ts.PreviousToken.Should().HaveType(TestTokenType.EndOfStream);
            ts.Position.Should().Be(0);

            var token = ts.Advance(10);
            TestTokenType.EndOfStream.Should().Be(token.TokenType);
            ts.IsEndOfStream().Should().BeTrue();
            ts.Position.Should().Be(0);

            token = ts.Advance(-100);
            TestTokenType.EndOfStream.Should().Be(token.TokenType);
            ts.Position.Should().Be(0);

            ts.CurrentToken.Should().HaveType(TestTokenType.EndOfStream);
            ts.NextToken.Should().HaveType(TestTokenType.EndOfStream);
            ts.PreviousToken.Should().HaveType(TestTokenType.EndOfStream);

            ts.Position = 0;
            ts.IsEndOfStream().Should().BeTrue();
            ts.Position.Should().Be(0);

            ts.MoveToNextToken();
            ts.IsEndOfStream().Should().BeTrue();
            ts.Position.Should().Be(0);
        }

        [Test]
        [Category.Languages.Core]
        public void TokenStreamTest1() {
            var tokens = new[]
            {
                new TestToken(TestTokenType.Token1, TextRange.FromBounds(0,1)),
                new TestToken(TestTokenType.Token2, TextRange.FromBounds(3,4)),
                new TestToken(TestTokenType.Token3, TextRange.FromBounds(6,8)),
                new TestToken(TestTokenType.Token1, TextRange.FromBounds(12,20)),
                new TestToken(TestTokenType.Token2, TextRange.FromBounds(20,21)),
            };

            var ts = CreateTokenStream(tokens);

            ts.Length.Should().Be(5);

            ts.IsEndOfStream().Should().BeFalse();
            ts.CurrentToken.Should().HaveType(TestTokenType.Token1);
            ts.NextToken.Should().HaveType(TestTokenType.Token2);
            ts.PreviousToken.Should().HaveType(TestTokenType.EndOfStream);
            ts.Position.Should().Be(0);

            ts.MoveToNextToken();

            ts.IsEndOfStream().Should().BeFalse();
            ts.CurrentToken.Should().HaveType(TestTokenType.Token2);
            ts.NextToken.Should().HaveType(TestTokenType.Token3);
            ts.PreviousToken.Should().HaveType(TestTokenType.Token1);
            ts.LookAhead(-2).Should().HaveType(TestTokenType.EndOfStream);
            ts.LookAhead(100).Should().HaveType(TestTokenType.EndOfStream);
            ts.Position.Should().Be(1);

            ts.Advance(2);

            ts.CurrentToken.Should().HaveType(TestTokenType.Token1);
            ts.NextToken.Should().HaveType(TestTokenType.Token2);
            ts.PreviousToken.Should().HaveType(TestTokenType.Token3);
            ts.LookAhead(-2).Should().HaveType(TestTokenType.Token2);
            ts.LookAhead(100).Should().HaveType(TestTokenType.EndOfStream);
            ts.Position.Should().Be(3);

            ts.MoveToNextToken();
            ts.MoveToNextToken();

            ts.IsEndOfStream().Should().BeTrue();
            ts.CurrentToken.Should().HaveType(TestTokenType.EndOfStream);
            ts.NextToken.Should().HaveType(TestTokenType.EndOfStream);
            ts.PreviousToken.Should().HaveType(TestTokenType.Token2);

            ts.MoveToNextToken();

            ts.IsEndOfStream().Should().BeTrue();
            ts.CurrentToken.Should().HaveType(TestTokenType.EndOfStream);
            ts.NextToken.Should().HaveType(TestTokenType.EndOfStream);
            ts.PreviousToken.Should().HaveType(TestTokenType.Token2);

            ts.Advance(-2);

            ts.CurrentToken.Should().HaveType(TestTokenType.Token1);
            ts.NextToken.Should().HaveType(TestTokenType.Token2);
            ts.PreviousToken.Should().HaveType(TestTokenType.Token3);
            ts.LookAhead(-2).Should().HaveType(TestTokenType.Token2);
            ts.LookAhead(100).Should().HaveType(TestTokenType.EndOfStream);
            ts.Position.Should().Be(3);
        }

        [Test]
        [Category.Languages.Core]
        public void TokenStreamLineBreakTest() {
            var tokens = new[]
            {
                new TestToken(TestTokenType.Token1, TextRange.FromBounds(0,1)),
                new TestToken(TestTokenType.Token2, TextRange.FromBounds(3,4)),
                new TestToken(TestTokenType.Token3, TextRange.FromBounds(6,8)),
                new TestToken(TestTokenType.Token1, TextRange.FromBounds(12,20)),
                new TestToken(TestTokenType.Token2, TextRange.FromBounds(20,21)),
            };

            var ts = CreateTokenStream(tokens);
            ITextProvider textProvider = new TextStream("1  2  11  \r\n12345678x");

            ts.IsLineBreakAfter(textProvider, ts.Position).Should().BeFalse();

            ts.Advance(2);
            ts.IsLineBreakAfter(textProvider, ts.Position).Should().BeTrue();

            ts.Advance(-1);
            ts.IsLineBreakAfter(textProvider, ts.Position).Should().BeFalse();

            ts.MoveToNextLine(textProvider);
            ts.CurrentToken.Should().HaveType(TestTokenType.Token1);

            string s = textProvider.GetText(ts.CurrentToken);
            s.Should().Be("12345678");
        }

        [Test]
        [Category.Languages.Core]
        public void TokenStreamEnumerationTest() {
            var tokens = new[]
            {
                new TestToken(TestTokenType.Token1, TextRange.FromBounds(0,1)),
                new TestToken(TestTokenType.Token2, TextRange.FromBounds(3,4)),
                new TestToken(TestTokenType.Token3, TextRange.FromBounds(6,8)),
                new TestToken(TestTokenType.Token1, TextRange.FromBounds(12,20)),
                new TestToken(TestTokenType.Token2, TextRange.FromBounds(20,21)),
            };

            var ts = CreateTokenStream(tokens);
            ts.Should().Equal(tokens);
        }

        private TokenStream<TestToken> CreateTokenStream(TestToken[] tokens) {
            return new TokenStream<TestToken>(
                 new TextRangeCollection<TestToken>(tokens),
                 new TestToken(TestTokenType.EndOfStream, TextRange.EmptyRange));
        }
    }
}
