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
    public class TokenizeConstantsTest : TokenizeTestBase<RToken, RTokenType> {
        [Test]
        public void Tokenize_Missing() {
            string s = "NA NA_character_ NA_complex_ NA_integer_ NA_real_";

            var tokens = Tokenize(s, new RTokenizer());

            tokens.Should().HaveCount(5);
            foreach (var token in tokens) {
                token.Should().HaveType(RTokenType.Missing).And.HaveSubType(RTokenSubType.BuiltinConstant);
            }
        }
    }
}
