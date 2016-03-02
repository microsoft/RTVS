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
            string expected =
@"function(a, b) {
  return(a + b)
}";
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
            string original =
@"x <- function (x,  
 intercept=TRUE, tolerance =1e-07, 
    yname = NULL)
";
            string actual = f.Format(original);
            string expected =
"x <- function(x,\r\n" +
" intercept = TRUE, tolerance = 1e-07,\r\n" +
"\t\tyname = NULL)\r\n";

            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionInlineScope01() {
            RFormatter f = new RFormatter();
            string actual = f.Format("x <- func(a,{return(b)})");
            string expected =
@"x <- func(a, {
  return(b)
})";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionInlineScope02() {
            RFormatter f = new RFormatter();
            string actual = f.Format("x <- func({return(b)})");
            string expected =
@"x <- func({
  return(b)
})";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionInlineIf01() {
            RFormatter f = new RFormatter();
            string actual = f.Format("x <- func(a,{if(TRUE) {x} else {y}})");
            string expected =
@"x <- func(a, {
  if (TRUE) {
    x
  } else {
    y
  }
})";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionInlineIf02() {
            RFormatter f = new RFormatter();
            string actual = f.Format("x <- func(a,{if(TRUE) 1 else 2})");
            string expected =
@"x <- func(a, {
  if (TRUE) 1 else 2
})";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionInlineIf03() {
            RFormatter f = new RFormatter();
            string original =
@"x <- func(a,{
if(TRUE) 1 else 2})";

            string actual = f.Format(original);
            string expected =
@"x <- func(a, {
  if (TRUE) 1 else 2
})";

            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionInlineIf04() {
            RFormatter f = new RFormatter();
            string original =
@"x <- func(a,{
if(TRUE) {1} else {2}})";

            string actual = f.Format(original);
            string expected =
@"x <- func(a, {
  if (TRUE) {
    1
  } else {
    2
  }
})";

            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionInlineIf05() {
            RFormatter f = new RFormatter();
            string original =
@"x <- func(a,{
        if(TRUE) {1} 
        else {2}
 })";

            string actual = f.Format(original);
            string expected =
@"x <- func(a, {
  if (TRUE) {
    1
  } else {
    2
  }
})";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionInlineIf06() {
            RFormatOptions options = new RFormatOptions();
            options.BracesOnNewLine = true;

            RFormatter f = new RFormatter(options);

            string original =
@"x <- func(a,
   {
      if(TRUE) 1 else 2
   })";

            string actual = f.Format(original);
            string expected =
@"x <- func(a,
   {
     if (TRUE) 1 else 2
   })";

            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionInlineIf07() {
            RFormatter f = new RFormatter();

            string original =
@"x <- func(a,
   {
      if(TRUE) 
        if(FALSE) {x <-1} else x<-2
else
        if(z) x <-1 else {5}
    })";

            string actual = f.Format(original);
            string expected =
@"x <- func(a, {
  if (TRUE)
    if (FALSE) {
      x <- 1
    } else
      x <- 2
  else
    if (z) x <- 1 else {
      5
    }
})";

            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFunctionNoSpaceAfterComma() {
            RFormatOptions options = new RFormatOptions();
            options.SpaceAfterComma = false;

            RFormatter f = new RFormatter(options);
            string actual = f.Format("function(a, b) {return(a+b)}");
            string expected =
@"function(a,b) {
  return(a + b)
}";
            actual.Should().Be(expected);
        }
    }
}
