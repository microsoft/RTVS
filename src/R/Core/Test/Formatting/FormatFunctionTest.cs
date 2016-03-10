// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Formatting;
using Microsoft.R.Core.Formatting;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Formatting {
    [ExcludeFromCodeCoverage]
    public class FormatFunctionTest {
        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunction() {
            RFormatter f = new RFormatter();
            string actual = f.Format("function(a,b) {return(a+b)}");
            string expected = "function(a, b) {\n  return(a + b)\n}";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatInlineFunction() {
            RFormatter f = new RFormatter();
            string actual = f.Format("function(a,b) a+b");
            string expected = @"function(a, b) a + b";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionAlignArguments() {
            RFormatOptions options = new RFormatOptions();
            options.IndentType = IndentType.Tabs;
            options.TabSize = 2;

            RFormatter f = new RFormatter(options);
            string original = "x <- function (x,  \n intercept=TRUE, tolerance =1e-07, \n    yname = NULL)\n";
            string actual = f.Format(original);
            string expected = "x <- function(x,\n intercept = TRUE, tolerance = 1e-07,\n\t\tyname = NULL)\n";

            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionInlineScope01() {
            RFormatter f = new RFormatter();
            string actual = f.Format("x <- func(a,{return(b)})");
            string expected = "x <- func(a, {\n  return(b)\n})";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionInlineScope02() {
            RFormatter f = new RFormatter();
            string actual = f.Format("x <- func({return(b)})");
            string expected = "x <- func({\n  return(b)\n})";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionInlineIf01() {
            RFormatter f = new RFormatter();
            string actual = f.Format("x <- func(a,{if(TRUE) {x} else {y}})");
            string expected = "x <- func(a, {\n  if (TRUE) {\n    x\n  } else {\n    y\n  }\n})";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionInlineIf02() {
            RFormatter f = new RFormatter();
            string actual = f.Format("x <- func(a,{if(TRUE) 1 else 2})");
            string expected = "x <- func(a, {\n  if (TRUE) 1 else 2\n})";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionInlineIf03() {
            RFormatter f = new RFormatter();
            string original = "x <- func(a,{\nif(TRUE) 1 else 2})";
            string actual = f.Format(original);
            string expected = "x <- func(a, {\n  if (TRUE) 1 else 2\n})";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionInlineIf04() {
            RFormatter f = new RFormatter();
            string original = "x <- func(a,{\nif(TRUE) {1} else {2}})";
            string actual = f.Format(original);
            string expected = "x <- func(a, {\n  if (TRUE) {\n    1\n  } else {\n    2\n  }\n})";

            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionInlineIf05() {
            RFormatter f = new RFormatter();
            string original = "x <- func(a,{\n        if(TRUE) {1} \n        else {2}\n })";
            string actual = f.Format(original);
            string expected = "x <- func(a, {\n  if (TRUE) {\n    1\n  } else {\n    2\n  }\n})";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionInlineIf06() {
            RFormatOptions options = new RFormatOptions();
            options.BracesOnNewLine = true;

            RFormatter f = new RFormatter(options);
            string original = "x <- func(a,\n   {\n      if(TRUE) 1 else 2\n   })";
            string actual = f.Format(original);
            string expected = "x <- func(a,\n   {\n     if (TRUE) 1 else 2\n   })";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionInlineIf07() {
            RFormatter f = new RFormatter();

            string original = "x <- func(a,\n   {\n      if(TRUE) \n        if(FALSE) {x <-1} else x<-2\nelse\n        if(z) x <-1 else {5}\n    })";
            string actual = f.Format(original);
            string expected =
"x <- func(a, {\n" +
"  if (TRUE)\n" +
"    if (FALSE) {\n" +
"      x <- 1\n" +
"    } else\n" +
"      x <- 2\n" +
"  else\n" +
"    if (z) x <- 1 else {\n" +
"      5\n" +
"    }\n" +
"})";

            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionNoSpaceAfterComma() {
            RFormatOptions options = new RFormatOptions();
            options.SpaceAfterComma = false;

            RFormatter f = new RFormatter(options);
            string actual = f.Format("function(a, b) {return(a+b)}");
            string expected = "function(a,b) {\n  return(a + b)\n}";
            actual.Should().Be(expected);
        }
    }
}
