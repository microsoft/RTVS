// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Core.Formatting;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Core.Test.Formatting {
    [ExcludeFromCodeCoverage]
    public class FormatTest {
        [CompositeTest]
        [Category.R.Formatting]
        [InlineData("if(1 && # comment\n   2) x", "if (1 && # comment\n   2) x")]
        [InlineData("if(1 && # comment\n   2)\n x", "if (1 && # comment\n   2)\n  x")]
        [InlineData("for(i in c('a', # comment\n 'b')) {}", "for (i in c('a', # comment\n 'b')) { }")]
        [InlineData("func(a,\n     b,\n     c)", "func(a,\n     b,\n     c)")]
        [InlineData("for(i in c(a,\n     b,\n     c)) {}", "for (i in c(a,\n     b,\n     c)) { }")]
        [InlineData("for(i in c(a , # comment\n    ))", "for (i in c(a, # comment\n    ))\n")]
        public void PreserveBreaks(string original, string expected) {
            RFormatter f = new RFormatter();
            string actual = f.Format(original);
            actual.Should().Be(expected);
        }
    }
}
