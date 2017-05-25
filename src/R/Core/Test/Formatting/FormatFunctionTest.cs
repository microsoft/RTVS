// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Formatting;
using Microsoft.R.Core.Formatting;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Core.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Category.R.Formatting]
    public class FormatFunctionTest {
        [CompositeTest]
        [InlineData("function(a,b) {\nreturn(a+b)}", "function(a, b) {\n  return(a + b)\n}")]
        [InlineData("function(a,b) {return(a+b)}", "function(a, b) { return(a + b) }")]
        [InlineData("function(a,b) {{return(a+b)}}", "function(a, b) {{ return(a + b) }}")]
        [InlineData("function(a,b) a+b", "function(a, b) a + b")]
        public void FormatFunction(string original, string expected) {
            var f = new RFormatter();
            var actual = f.Format(original);
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatFunctionAlignArguments() {
            var options = new RFormatOptions() {
                IndentType = IndentType.Tabs,
                TabSize = 2
            };
            var f = new RFormatter(options);
            var original = "x <- function (x,  \n intercept=TRUE, tolerance =1e-07, \n    yname = NULL)\n";
            var actual = f.Format(original);
            var expected = "x <- function(x,\n intercept = TRUE, tolerance = 1e-07,\n\t\tyname = NULL)\n";

            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("x <- func(a,{return(b)})", "x <- func(a, { return(b) })")]
        [InlineData("x <- func(a,{return(b)\n})", "x <- func(a, {\n  return(b)\n})")]
        [InlineData("x<-func({return(b)})", "x <- func({ return(b) })")]
        [InlineData("x<-func({\nreturn(b)})", "x <- func({\n  return(b)\n})")]
        public void FunctionInlineScope(string original, string expected) {
            var f = new RFormatter();
            var actual = f.Format(original);
            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("x <- func(a,{if(TRUE) {x} else {y}})", "x <- func(a, { if (TRUE) { x } else { y }})")]
        [InlineData("x <- func(a,{if(TRUE) 1 else 2})", "x <- func(a, { if (TRUE) 1 else 2 })")]
        [InlineData("x <- func(a,{\nif(TRUE) 1 else 2})", "x <- func(a, {\n  if (TRUE) 1 else 2\n})")]
        [InlineData("x <- func(a,{\nif(TRUE) {1} else {2}})", "x <- func(a, {\n  if (TRUE) { 1 } else { 2 }\n})")]
        [InlineData("x <- func(a,{\n        if(TRUE) {1} \n        else {2}\n })", "x <- func(a, {\n  if (TRUE) { 1 }\n  else { 2 }\n})")]
        [InlineData("x <- func(a,\n   {\n      if(TRUE) 1 else 2\n   })", "x <- func(a, {\n  if (TRUE) 1 else 2\n})")]
        public void FunctionInlineIf(string original, string expected) {
            var f = new RFormatter();
            var actual = f.Format(original);
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatFunctionInlineIf02() {
            var f = new RFormatter();

            var original = "x <- func(a,\n   {\n      if(TRUE) \n        if(FALSE) {x <-1} else x<-2\nelse\n        if(z) x <-1 else {5}\n    })";
            var actual = f.Format(original);
            var expected =
"x <- func(a, {\n" +
"  if (TRUE)\n" +
"    if (FALSE) { x <- 1 } else x <- 2\n" +
"  else\n" +
"    if (z) x <- 1 else { 5 }\n" +
"})";

            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("function(a, b) {return(a+b)}", "function(a,b) { return(a + b) }")]
        [InlineData("function(a, b) {return(a+b)\n}", "function(a,b) {\n  return(a + b)\n}")]
        public void FormatFunctionNoSpaceAfterComma(string original, string expected) {
            var options = new RFormatOptions() {
                SpaceAfterComma = false
            };
            var f = new RFormatter(options);
            var actual = f.Format(original);
            actual.Should().Be(expected);
        }
    }
}
