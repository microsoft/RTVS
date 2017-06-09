// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Editor.RData.Tokens;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Editor.RData.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class VerifySortedRdTables : TokenizeTestBase<RdToken, RdTokenType> {
        [Test]
        [Category.Rd.Tokenizer]
        public void VerifySortedRdBlockKeywords() {
            string[] array = new List<string>(RdBlockContentType.Keywords).ToArray();
            Array.Sort(array);

            array.Should().Equal(RdBlockContentType.Keywords);
        }

        [Test]
        [Category.Rd.Tokenizer]
        public void VerifySortedRdVerbatimKeywords() {
            string[] array = new List<string>(RdBlockContentType.VerbatimKeywords).ToArray();
            Array.Sort(array);

            array.Should().Equal(RdBlockContentType.VerbatimKeywords);
        }
    }
}
