// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Formatting;
using Microsoft.R.Core.Formatting;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Category.R.Formatting]
    public class FormatConditionalsTest {
        [Test]
        public void FormatConditionalTest01() {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true){if(false){}}");
            string expected = "if (true) {\n  if (false) { }\n}";
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatConditionalTest02() {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(a == a+((b+c)/x)){if(func(a,b, c+2, x=2, ...)){}}");
            string expected = "if (a == a + ((b + c) / x)) {\n  if (func(a, b, c + 2, x = 2, ...)) { }\n}";
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatConditionalTest03() {
            RFormatOptions options = new RFormatOptions();
            options.BracesOnNewLine = true;
            options.IndentSize = 2;
            options.IndentType = IndentType.Tabs;
            options.TabSize = 2;

            RFormatter f = new RFormatter(options);
            string actual = f.Format("if(a == a+((b+c)/x)){if(func(a,b, c+2, x=2, ...)){}}");
            string expected = "if (a == a + ((b + c) / x))\n{\n\tif (func(a, b, c + 2, x = 2, ...)) { }\n}";
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatConditionalTest04() {
            RFormatOptions options = new RFormatOptions();
            options.BracesOnNewLine = true;

            RFormatter f = new RFormatter(options);
            string actual = f.Format("if(TRUE) { 1 } else {2} x<-1");
            string expected = "if (TRUE)\n{\n  1\n} else\n{\n  2\n}\nx <- 1";
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatConditionalTest05() {
            RFormatOptions options = new RFormatOptions();
            options.BracesOnNewLine = true;

            RFormatter f = new RFormatter(options);
            string actual = f.Format("if(TRUE) { 1 } else if(FALSE) {2} else {3} x<-1");
            string expected = "if (TRUE)\n{\n  1\n} else if (FALSE)\n{\n  2\n} else\n{\n  3\n}\nx <- 1";
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatNoCurlyConditionalTest01() {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true) x<-2");
            string expected = "if (true)\n  x <- 2";
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatNoCurlyConditionalTest02() {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true) x<-2 else x<-1");
            string expected = "if (true) x <- 2 else x <- 1";
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatNoCurlyConditionalTest03() {
            RFormatOptions options = new RFormatOptions();
            options.IndentType = IndentType.Tabs;

            RFormatter f = new RFormatter(options);
            string actual = f.Format("if(true)    x<-2");
            string expected = "if (true)\n\tx <- 2";
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatNoCurlyConditionalTest04() {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true) if(false)   x<-2");
            string expected = "if (true)\n  if (false)\n    x <- 2";
            actual.Should().Be(expected);
        }

        [Test]
        [Category.R.Formatting]
        public void FormatNoCurlyConditionalTest05() {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true) if(false)   x<-2 else {1}");
            string expected = "if (true)\n  if (false) x <- 2 else {\n    1\n  }";
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatNoCurlyConditionalTest06() {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true) repeat { x <-1; next;} else z");
            string expected = "if (true)\n  repeat {\n    x <- 1;\n    next;\n  } else\n  z";
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatNoCurlyConditionalTest07() {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true) if(false) {  x<-2 } else 1");
            string expected = "if (true)\n  if (false) {\n    x <- 2\n  } else\n    1";
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatNoCurlyRepeatTest01() {
            RFormatter f = new RFormatter();
            string actual = f.Format("repeat x<-2");
            string expected = "repeat\n  x <- 2";
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatConditionalAlignBraces01() {
            RFormatter f = new RFormatter();
            string original = "\n    if (intercept) \n\t{\n        x <- cbind(1, x)\n    }\n";
            string actual = f.Format(original);
            string expected = "\nif (intercept) {\n  x <- cbind(1, x)\n}\n";
            actual.Should().Be(expected);
        }

        [Test]
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
        public void FormatForTest() {
            RFormatter f = new RFormatter();
            string original = @"for (i in  1:6) x[,i]= rowMeans(fmri[[i]])";

            string actual = f.Format(original);
            string expected = "for (i in 1:6)\n  x[, i] = rowMeans(fmri[[i]])";
            actual.Should().Be(expected);
        }
    }
}
