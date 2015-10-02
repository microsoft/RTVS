using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Formatting
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FormatFunctionTest : UnitTestBase
    {
        [TestMethod]
        public void Formatter_FormatFunction()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("function(a,b) {return(a+b)}");
            string expected =
@"function(a, b) {
    return (a + b)
}";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatInlineFunction()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("function(a,b) a+b");
            string expected = @"function(a, b) a + b";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatFunctionAlignArguments()
        {
            RFormatOptions options = new RFormatOptions();
            options.IndentType = IndentType.Tabs;

            RFormatter f = new RFormatter(options);
            string original =
@"x <- function (x,  
 intercept=TRUE, tolerance =1e-07, 
    yname = NULL)
";
            string actual = f.Format(original);
            string expected =
@"x <- function(x,
 intercept = TRUE, tolerance = 1e-07,
	yname = NULL)
";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatFunctionInlineScope01()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("x <- func(a,{return(b)})");
            string expected = 
@"x <- func(a, {
    return (b)
})";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatFunctionInlineScope02()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("x <- func({return(b)})");
            string expected =
@"x <- func({
    return (b)
})";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatFunctionInlineIf01()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("x <- func(a,{if(TRUE) {x} else {y}})");
            string expected = 
@"x <- func(a, {
    if (TRUE) {
        x
    }
    else {
        y
    }
})";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatFunctionInlineIf02()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("x <- func(a,{if(TRUE) 1 else 2})");
            string expected = 
@"x <- func(a, {
    if (TRUE) 1 else 2
})";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatFunctionInlineIf03()
        {
            RFormatter f = new RFormatter();
            string original =
@"x <- func(a,{
if(TRUE) 1 else 2})";

            string actual = f.Format(original);
            string expected = 
@"x <- func(a, {
    if (TRUE) 1 else 2
})";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatFunctionInlineIf04()
        {
            RFormatter f = new RFormatter();
            string original =
@"x <- func(a,{
if(TRUE) {1} else {2}})";

            string actual = f.Format(original);
            string expected =
@"x <- func(a, {
    if (TRUE) {
        1
    }
    else {
        2
    }
})";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatFunctionInlineIf05()
        {
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
    }
    else {
        2
    }
})";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatFunctionInlineIf06()
        {
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

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatFunctionInlineIf07()
        {
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
        }
        else
            x <- 2
    else
        if (z) x <- 1 else {
            5
        }
})";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatFunctionNoSpaceAfterComma()
        {
            RFormatOptions options = new RFormatOptions();
            options.SpaceAfterComma = false;

            RFormatter f = new RFormatter(options);
            string actual = f.Format("function(a, b) {return(a+b)}");
            string expected =
@"function(a,b) {
    return (a + b)
}";
            Assert.AreEqual(expected, actual);
        }
    }
}
