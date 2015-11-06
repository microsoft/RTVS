using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Formatting
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FormatConditionalsTest : UnitTestBase
    {
        [TestMethod]
        public void Formatter_FormatConditionalTest01()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true){if(false){}}");
            string expected =
@"if (true) {
  if (false) { }
}";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatConditionalTest02()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(a == a+((b+c)/x)){if(func(a,b, c+2, x=2, ...)){}}");
            string expected =
@"if (a == a + ((b + c) / x)) {
  if (func(a, b, c + 2, x = 2, ...)) { }
}";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatConditionalTest03()
        {
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
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatConditionalTest04()
        {
            RFormatOptions options = new RFormatOptions();
            options.BracesOnNewLine = true;

            RFormatter f = new RFormatter(options);
            string actual = f.Format("if(TRUE) { 1 } else {2} x<-1");
            string expected =
@"if (TRUE)
{
  1
}
else
{
  2
}
x <- 1";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatConditionalTest05()
        {
            RFormatOptions options = new RFormatOptions();
            options.BracesOnNewLine = true;

            RFormatter f = new RFormatter(options);
            string actual = f.Format("if(TRUE) { 1 } else if(FALSE) {2} else {3} x<-1");
            string expected =
@"if (TRUE)
{
  1
}
else if (FALSE)
{
  2
}
else
{
  3
}
x <- 1";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatNoCurlyConditionalTest01()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true) x<-2");
            string expected =
@"if (true)
  x <- 2";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatNoCurlyConditionalTest02()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true) x<-2 else x<-1");
            string expected =
@"if (true) x <- 2 else x <- 1";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatNoCurlyConditionalTest03()
        {
            RFormatOptions options = new RFormatOptions();
            options.IndentType = IndentType.Tabs;

            RFormatter f = new RFormatter(options);
            string actual = f.Format("if(true)    x<-2");
            string expected =
@"if (true)
	x <- 2";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatNoCurlyConditionalTest04()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true) if(false)   x<-2");
            string expected =
@"if (true)
  if (false)
    x <- 2";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatNoCurlyConditionalTest05()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true) if(false)   x<-2 else {1}");
            string expected =
@"if (true)
  if (false) x <- 2 else {
    1
  }";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatNoCurlyConditionalTest06()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true) repeat { x <-1; next;} else z");
            string expected =
@"if (true)
  repeat {
    x <- 1;
    next;
  }
else
  z";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatNoCurlyConditionalTest07()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true) if(false) {  x<-2 } else 1");
            string expected =
@"if (true)
  if (false) {
    x <- 2
  } else
    1";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatNoCurlyRepeatTest01()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("repeat x<-2");
            string expected =
@"repeat
  x <- 2";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatConditionalAlignBraces01()
        {
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
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatConditionalAlignBraces02()
        {
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
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_PreserveEmptyLines()
        {
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
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_AlignComments()
        {
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
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatForTest()
        {
            RFormatter f = new RFormatter();
            string original = @"for (i in 1:6) x[, i] = rowMeans(fmri[[i]])";

            string actual = f.Format(original);

            string expected =
@"for (i in 1:6)
  x[, i] = rowMeans(fmri[[i]])";

            Assert.AreEqual(expected, actual);
        }
    }
}
