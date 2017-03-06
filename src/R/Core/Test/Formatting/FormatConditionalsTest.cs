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
    public class FormatConditionalsTest {
        [CompositeTest]
        [InlineData("if(true){if(false){}}", "if (true) { if (false) { }}")]
        [InlineData("if(true){\nif(false){}}", "if (true) {\n  if (false) { }\n}")]
        public void ConditionalTest(string original, string expected) {
            RFormatter f = new RFormatter();
            string actual = f.Format(original);
            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("if(a == a+((b+c)/x)){if(func(a,b, c+2, x=2, ...)){}}", "if (a == a + ((b + c) / x)) { if (func(a, b, c + 2, x = 2, ...)) { }}")]
        [InlineData("if(a == a+((b+c)/x)){\nif(func(a,b, c+2, x=2, ...)){}}", "if (a == a + ((b + c) / x)) {\n  if (func(a, b, c + 2, x = 2, ...)) { }\n}")]
        public void ConditionalTest02(string original, string expected) {
            RFormatter f = new RFormatter();
            string actual = f.Format(original);
            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("if(a == a+((b+c)/x)){if(func(a,b, c+2, x=2, ...)){}}", 
                    "if (a == a + ((b + c) / x)) { if (func(a, b, c + 2, x = 2, ...)) { }}")]
        [InlineData("if(a == a+((b+c)/x)){if(func(a,b, c+2, x=2, ...)){}\n}",
                    "if (a == a + ((b + c) / x))\n{\n\tif (func(a, b, c + 2, x = 2, ...)) { }\n}")]
        public void ConditionalTest03(string original, string expected) {
            RFormatOptions options = new RFormatOptions();
            options.BracesOnNewLine = true;
            options.IndentSize = 2;
            options.IndentType = IndentType.Tabs;
            options.TabSize = 2;

            RFormatter f = new RFormatter(options);
            string actual = f.Format(original);
            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("if(TRUE) { 1 } else {2} x<-1", "if (TRUE) { 1 } else { 2 }\nx <- 1")]
        [InlineData("if(TRUE) {\n1 } else {2} x<-1", "if (TRUE)\n{\n  1\n} else { 2 }\nx <- 1")]
        [InlineData("if(TRUE) {\n1 } else {2\n} x<-1", "if (TRUE)\n{\n  1\n} else\n{\n  2\n}\nx <- 1")]
        public void ConditionalTest04(string original, string expected) {
            RFormatOptions options = new RFormatOptions();
            options.BracesOnNewLine = true;

            RFormatter f = new RFormatter(options);
            string actual = f.Format(original);
            actual.Should().Be(expected);
        }

        [Test]
        public void ConditionalTest05() {
            RFormatOptions options = new RFormatOptions();
            options.BracesOnNewLine = true;

            RFormatter f = new RFormatter(options);
            string actual = f.Format("if(TRUE) { 1 } else if(FALSE) {2} else {3} x<-1");
            string expected = "if (TRUE) { 1 } else if (FALSE) { 2 } else { 3 }\nx <- 1";
            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("if(TRUE){}", true, "if (TRUE) { }")]
        [InlineData("if(TRUE){}", false, "if (TRUE){ }")]
        [InlineData("if(TRUE){\n}", true, "if (TRUE) {\n}")]
        [InlineData("if(TRUE){\n}", false, "if (TRUE){\n}")]
        public void ConditionalTest06(string original, bool spaceBeforeCurly, string expected) {
            var options = new RFormatOptions() {
                SpaceBeforeCurly = spaceBeforeCurly
            };
            RFormatter f = new RFormatter(options);
            string actual = f.Format(original);
            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("if(true) x<-2", "if (true) x <- 2")]
        [InlineData("if(true)\nx<-2", "if (true)\n  x <- 2")]
        [InlineData("if(true) x<-2 else x<-1", "if (true) x <- 2 else x <- 1")]
        [InlineData("if(true)\nx<-2 else x<-1", "if (true)\n  x <- 2 else x <- 1")]
        [InlineData("if(true) if(false)   x<-2", "if (true) if (false) x <- 2")]
        [InlineData("if(true) if(false)   x<-2 else {1}", "if (true) if (false) x <- 2 else { 1 }")]
        [InlineData("if(true) if(false)   x<-2 else {1\n}", "if (true) if (false) x <- 2 else {\n  1\n}")]
        [InlineData("if(true) repeat { x <-1; next;} else z", "if (true) repeat { x <- 1; next; } else z")]
        [InlineData("if(true) repeat { x <-1; \nnext;} else z", "if (true) repeat {\n  x <- 1;\n  next;\n} else z")]
        [InlineData("if(true) if(false) {  x<-2 } else 1", "if (true) if (false) { x <- 2 } else 1")]
        [InlineData("if(true)\n if(false) {  x<-2 } else 1", "if (true)\n  if (false) { x <- 2 } else 1")]
        [InlineData("if(true)\n if(false) {  x<-2\n } else \n1", "if (true)\n  if (false) {\n    x <- 2\n  } else\n    1")]
        public void FormatNoCurlyConditionalTest(string original, string expected) {
            RFormatter f = new RFormatter();
            string actual = f.Format(original);
            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("repeat x<-2", "repeat x <- 2")]
        [InlineData("repeat\nx<-2", "repeat\n  x <- 2")]
        public void NoCurlyRepeatTest(string original, string expected) {
            RFormatter f = new RFormatter();
            string actual = f.Format(original);
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
