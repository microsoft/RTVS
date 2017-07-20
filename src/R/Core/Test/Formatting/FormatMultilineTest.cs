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
    public class FormatMultilineTest {
        [CompositeTest]
        [InlineData("x %>% y%>%\n   z%>%a", "x %>% y %>%\n   z %>% a")]
        [InlineData("((x %>% y)\n   %>%z%>%a)", "((x %>% y)\n   %>% z %>% a)")]
        [InlineData("x <- function()\n  z", "x <- function()\n  z")]
        [InlineData("{\n  x <- function()\n      z\n}", "{\n  x <- function()\n      z\n}")]
        [InlineData("x <- \n  if(TRUE) {\n   z\n}", "x <-\n  if (TRUE) {\n    z\n  }")]
        [InlineData("{\n  x <- \n    if(TRUE) {\n   z\n}\n}", "{\n  x <-\n    if (TRUE) {\n      z\n    }\n}")]
        [InlineData("x <-function(a,\n    b){\n z\n}", "x <- function(a,\n    b) {\n  z\n}")]
        public void Multiline(string original, string expected) {
            var f = new RFormatter();
            var actual = f.Format(original);
            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("x <-1\n# comment", "x <- 1\n# comment\n")]
        [InlineData("{\nx <-1\n# comment\n}", "{\n  x <- 1\n  # comment\n}")]
        [InlineData("{\nx <-1\n# comment\n # comment\n         # comment\n y<-2\n}",
                    "{\n  x <- 1\n  # comment\n  # comment\n  # comment\n  y <- 2\n}")]
        public void Comments(string original, string expected) {
            var f = new RFormatter();
            var actual = f.Format(original);
            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("x <-1;y<-2", true, "x <- 1;\ny <- 2")]
        [InlineData("x <-1;y<-2", false, "x <- 1; y <- 2")]
        [InlineData("x <-1;\ny<-2", true, "x <- 1;\ny <- 2")]
        [InlineData("x <-1;\ny<-2", false, "x <- 1;\ny <- 2")]
        public void Statements(string original, bool breakMultipleStatements, string expected) {
            var options = new RFormatOptions { BreakMultipleStatements = breakMultipleStatements };
            var f = new RFormatter(options);
            var actual = f.Format(original);
            actual.Should().Be(expected);
        }
    }
}
