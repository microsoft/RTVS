// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Formatting;
using Microsoft.R.Core.Formatting;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Formatting {
    [ExcludeFromCodeCoverage]
    public class FormatConditionalsTest {
        [Test]
        [Category.R.Formatting]
        public void FormatConditionalTest01() {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true){if(false){}}");
            string expected =
@"if (true) {
  if (false) { }
}";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void FormatConditionalTest02() {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(a == a+((b+c)/x)){if(func(a,b, c+2, x=2, ...)){}}");
            string expected =
@"if (a == a + ((b + c) / x)) {
  if (func(a, b, c + 2, x = 2, ...)) { }
}";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void FormatConditionalTest03() {
            RFormatOptions options = new RFormatOptions();
            options.BracesOnNewLine = true;
            options.IndentSize = 2;
            options.IndentType = IndentType.Tabs;
            options.TabSize = 2;

            RFormatter f = new RFormatter(options);
            string actual = f.Format("if(a == a+((b+c)/x)){if(func(a,b, c+2, x=2, ...)){}}");
            string expected =
@"if (a == a + ((b + c) / x))
{
	if (func(a, b, c + 2, x = 2, ...)) { }
}";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Ast]
        public void FormatConditionalTest04() {
            RFormatOptions options = new RFormatOptions();
            options.BracesOnNewLine = true;

            RFormatter f = new RFormatter(options);
            string actual = f.Format("if(TRUE) { 1 } else {2} x<-1");
            string expected =
@"if (TRUE)
{
  1
} else
{
  2
}
x <- 1";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void FormatConditionalTest05() {
            RFormatOptions options = new RFormatOptions();
            options.BracesOnNewLine = true;

            RFormatter f = new RFormatter(options);
            string actual = f.Format("if(TRUE) { 1 } else if(FALSE) {2} else {3} x<-1");
            string expected =
@"if (TRUE)
{
  1
} else if (FALSE)
{
  2
} else
{
  3
}
x <- 1";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void FormatNoCurlyConditionalTest01() {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true) x<-2");
            string expected =
@"if (true)
  x <- 2";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void FormatNoCurlyConditionalTest02() {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true) x<-2 else x<-1");
            string expected =
@"if (true) x <- 2 else x <- 1";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void FormatNoCurlyConditionalTest03() {
            RFormatOptions options = new RFormatOptions();
            options.IndentType = IndentType.Tabs;

            RFormatter f = new RFormatter(options);
            string actual = f.Format("if(true)    x<-2");
            string expected =
@"if (true)
	x <- 2";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void FormatNoCurlyConditionalTest04() {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true) if(false)   x<-2");
            string expected =
@"if (true)
  if (false)
    x <- 2";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void FormatNoCurlyConditionalTest05() {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true) if(false)   x<-2 else {1}");
            string expected =
@"if (true)
  if (false) x <- 2 else {
    1
  }";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void FormatNoCurlyConditionalTest06() {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true) repeat { x <-1; next;} else z");
            string expected =
@"if (true)
  repeat {
    x <- 1;
    next;
  } else
  z";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void FormatNoCurlyConditionalTest07() {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true) if(false) {  x<-2 } else 1");
            string expected =
@"if (true)
  if (false) {
    x <- 2
  } else
    1";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void FormatNoCurlyRepeatTest01() {
            RFormatter f = new RFormatter();
            string actual = f.Format("repeat x<-2");
            string expected =
@"repeat
  x <- 2";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void FormatConditionalAlignBraces01() {
            RFormatter f = new RFormatter();
            string original =
@"
    if (intercept) 
	{
        x <- cbind(1, x)
    }
";
            string actual = f.Format(original);
            string expected =
@"
if (intercept) {
  x <- cbind(1, x)
}
";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void FormatConditionalAlignBraces02() {
            RFormatter f = new RFormatter();
            string original =
@"
    if (intercept) 
	{
        if(x>1)
x<- 1
else
if(x>2)
{
x<-3
}
    }
";
            string actual = f.Format(original);
            string expected =
@"
if (intercept) {
  if (x > 1)
    x <- 1
  else
    if (x > 2) {
      x <- 3
    }
}
";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void PreserveEmptyLines() {
            RFormatter f = new RFormatter();
            string original =
@"
    if (intercept) 
	{
x<- 1

x<-3
}
";
            string actual = f.Format(original);
            string expected =
@"
if (intercept) {
  x <- 1

  x <- 3
}
";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void AlignComments() {
            RFormatter f = new RFormatter();
            string original =
@"
        # comment1
    if (intercept) 
	{
x<- 1

# comment2
x<-3
}
";
            string actual = f.Format(original);
            string expected =
@"
# comment1
if (intercept) {
  x <- 1

  # comment2
  x <- 3
}
";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void FormatForTest() {
            RFormatter f = new RFormatter();
            string original = @"for (i in 1:6) x[, i] = rowMeans(fmri[[i]])";

            string actual = f.Format(original);

            string expected =
@"for (i in 1:6)
  x[, i] = rowMeans(fmri[[i]])";

            actual.Should().Be(expected);
        }
    }
}
