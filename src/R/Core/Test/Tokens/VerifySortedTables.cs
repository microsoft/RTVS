// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.R.Core.Tokens;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class VerifySortedTables : TokenizeTestBase<RToken, RTokenType> {
        [Test]
        [Category.R.Formatting]
        public void VerifySortedRKeywords() {
            string[] array = new List<string>(Keywords.KeywordList).ToArray();
            Array.Sort(array);

            array.Should().Equal(Keywords.KeywordList);
        }

        [Test]
        [Category.R.Formatting]
        public void VerifySorted2CharOperators() {
            string[] array = new List<string>(Operators._twoChars).ToArray();
            Array.Sort(array);

            array.Should().Equal(Operators._twoChars);
        }

        [Test]
        [Category.R.Formatting]
        public void VerifySorted3CharOperators() {
            string[] array = new List<string>(Operators._threeChars).ToArray();
            Array.Sort(array);

            array.Should().Equal(Operators._threeChars);
        }
    }
}
