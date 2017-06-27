// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    [Category.R.Tokenizer]
    public class TokenizeBuiltinsTest : TokenizeTestBase<RToken, RTokenType> {
        [Test]
        public void Tokenize_BuiltIns01() {
            var tokens = Tokenize("require library switch return", new RTokenizer());

            tokens.Should().HaveCount(4);
            foreach (var token in tokens) {
                token.Should().HaveType(RTokenType.Identifier).And.HaveSubType(RTokenSubType.BuiltinFunction);
            }
        }

        [Test]
        public void Tokenize_BuiltIns02() {
            var tokens = Tokenize("require() library() switch() return()", new RTokenizer());

            tokens.Should().HaveCount(12);
            for (var i = 0; i < tokens.Count; i += 3) {
                tokens[i].Should().HaveType(RTokenType.Identifier).And.HaveSubType(RTokenSubType.BuiltinFunction);
                tokens[i + 1].Should().HaveType(RTokenType.OpenBrace);
                tokens[i + 2].Should().HaveType(RTokenType.CloseBrace);
            }
        }
    }
}
