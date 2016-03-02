// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class TokenizeRandomStringsTest : TokenizeTestBase<RToken, RTokenType> {
        [Test]
        [Category.R.Tokenizer]
        public void Tokenize_NonEnglishString01() {
            var tokens = Tokenize(" русский ", new RTokenizer());

            tokens.Should().ContainSingle()
                .Which.Should().HaveType(RTokenType.Unknown)
                .And.StartAt(1)
                .And.HaveLength(7);
        }
    }
}
