// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Core.Formatting;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Core.Test.Formatting {
    [ExcludeFromCodeCoverage]
    public class FormatLineBreaksTest {
        [CompositeTest]
        [Category.R.Formatting]
        [InlineData("if(1 && # comment\n   2) x", "if (1 && # comment\n   2)\n  x")]
        [InlineData("func(a,\n     b,\n     c)", "func(a,\n     b,\n     c)")]
        [InlineData("for(i in c(a,\n     b,\n     c)) {}", "for (i in c(a,\n     b,\n     c)) { }")]
        public void PreserveBreaks(string original, string expected) {
            RFormatter f = new RFormatter();
            string actual = f.Format(original);
            actual.Should().Be(expected);
        }
    }
}
