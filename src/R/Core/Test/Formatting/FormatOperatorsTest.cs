// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Core.Formatting;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Core.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Category.R.Formatting]
    public class FormatOperatorsTest {
        [CompositeTest]
        [InlineData("-1", "-1")]
        [InlineData("- 1", "-1")]
        [InlineData("x--1", "x - -1")]
        [InlineData("-x-1", "-x - 1")]
        [InlineData("??plot", "??plot")]
        [InlineData("?plot", "?plot")]
        [InlineData("x?plot", "x ? plot")]
        [InlineData("x <-(-y+2)", "x <- (-y + 2)")]
        [InlineData("x <--+-1", "x <- -+-1")]
        [InlineData("x <- a--++1", "x <- a - -++1")]
        [InlineData("x <- a(b(??topic,c)", "x <- a(b(??topic, c)")]
        public void Formatter_FormatUnary(string original, string expected) {
            RFormatter f = new RFormatter();
            string actual = f.Format(original);
            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("x=1", true, "x = 1")]
        [InlineData("x=1", false, "x=1")]
        [InlineData("y <- 'x=1'", false, "y <- 'x=1'")]
        [InlineData("y <- 'x=1'", true, "y <- 'x=1'")]
        [InlineData("x <- function(a=1,b=2)", true, "x <- function(a = 1, b = 2)")]
        [InlineData("x <- function(a=1,b=2)", false, "x <- function(a=1, b=2)")]
        public void Formatter_FormatEquals(string original, bool spaceAroundEquals, string expected) {
            var options = new RFormatOptions() {
                SpacesAroundEquals = spaceAroundEquals
            };
            RFormatter f = new RFormatter(options);
            string actual = f.Format(original);
            actual.Should().Be(expected);
        }
    }
}
