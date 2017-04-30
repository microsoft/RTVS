// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Editor.RData.Tokens;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Editor.Test.RData.Tokens {
    [ExcludeFromCodeCoverage]
    [Category.Rd.Tokenizer]
    public class TokenizeRdTest : TokenizeTestBase<RdToken, RdTokenType> {
        [Test]
        public void TokenizeRdKeywords1() {
            var tokens = Tokenize(@" \title", new RdTokenizer());
            tokens.Should().ContainSingle()
                .Which.Should().Be(RdTokenType.Keyword, 1, 6);
        }

        [Test]
        public void TokenizeRdKeywords2() {
            var tokens = Tokenize(@" \title{}", new RdTokenizer());
            tokens.Should().HaveCount(3);
            tokens[0].Should().Be(RdTokenType.Keyword, 1, 6);
            tokens[1].Should().Be(RdTokenType.OpenCurlyBrace, 7, 1);
            tokens[2].Should().Be(RdTokenType.CloseCurlyBrace, 8, 1);
        }

        [Test]
        public void TokenizeRdPragmas1() {
            var tokens = Tokenize("#ifdef\ntext\n#endif", new RdTokenizer());
            tokens.Should().HaveCount(2);
            tokens[0].Should().Be(RdTokenType.Pragma, 0, 6);
            tokens[1].Should().Be(RdTokenType.Pragma, 12, 6);
        }

        [Test]
        public void TokenizeRdPragmas2() {
            var tokens = Tokenize(" #if\ntext\n #endif", new RdTokenizer());
            tokens.Should().BeEmpty();
        }

        [Test]
        public void TokenizeRdArguments01() {
            var actualTokens = Tokenize(@"\a1{arg[text \a1[=a2]] text}", new RdTokenizer());
            var expectedTokens = new[]
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


        [Test]
        public void TokenizeRdArguments02() {
            var actualTokens = Tokenize(@"\method{as.matrix}{data.frame}(x)", new RdTokenizer());
            var expectedTokens = new[]
            {
                new TokenData<RdTokenType>(RdTokenType.Keyword, 0, 7),
                new TokenData<RdTokenType>(RdTokenType.OpenCurlyBrace, 7, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseCurlyBrace, 17, 1),
                new TokenData<RdTokenType>(RdTokenType.OpenCurlyBrace, 18, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseCurlyBrace, 29, 1),
            };

            TokensCompare<RdTokenType, RdToken>.Compare(expectedTokens, actualTokens);
        }

        [Test]
        public void TokenizeRdArguments03() {
            var actualTokens = Tokenize(@"\usage{\method{as.matrix}{data.frame}(x)}", new RdTokenizer());
            var expectedTokens = new[]
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

        [Test]
        public void TokenizeRdArguments04() {
            var actualTokens = Tokenize(@"\ifelse{{latex}{\out[x]{~}}{ }}{}", new RdTokenizer());
            var expectedTokens = new[]
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

        [Test]
        public void TokenizeRdArguments05() {
            var actualTokens = Tokenize(@"\item{\dots}{ A }", new RdTokenizer());
            var expectedTokens = new[]
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

        [Test]
        public void TokenizeRdVerbationContent() {
            var actualTokens = Tokenize(
@"\alias{\% \dots %foo}
#ifdef
%comment", new RdTokenizer());
            var expectedTokens = new[]
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

        [Test]
        public void TokenizeCppFormatContent() {
            var actualTokens = Tokenize("\\usage{bmp(filename = \"Rplot%03d.jpg\", width = 480)}", new RdTokenizer(false));
            actualTokens.Should().HaveCount(3);
            var expectedTokens = new[]
            {
                new TokenData<RdTokenType>(RdTokenType.Keyword, 0, 6),
                new TokenData<RdTokenType>(RdTokenType.OpenCurlyBrace, 6, 1),
                new TokenData<RdTokenType>(RdTokenType.CloseCurlyBrace, 51, 1)
            };
            TokensCompare<RdTokenType, RdToken>.Compare(expectedTokens, actualTokens);
        }
    }
}
