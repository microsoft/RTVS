using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Expressions.Definitions;
using Microsoft.R.Core.AST.Functions.Definitions;
using Microsoft.R.Core.AST.Operators.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Formatting
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FormatterTest : UnitTestBase
    {
        [TestMethod]
        public void Formatter_EmptyFileTest()
        {
            RFormatter f = new RFormatter();
            string s = f.Format(string.Empty);
            Assert.AreEqual(0, s.Length);
        }

        [TestMethod]
        public void Formatter_FormatSimpleScopesTest01()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("{{}}");
            string expected =
@"{
    {
    }
}";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatConditionalTest01()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(true){if(false){}}");
            string expected =
@"if (true) {
    if (false) {
    }
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
    if (func(a, b, c + 2, x = 2, ...)) {
    }
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
	if (func(a, b, c + 2, x = 2, ...))
	{
	}
}";
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
        public void Formatter_StatementTest01()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("x<-2");
            string expected =
@"x <- 2";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatFunction()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("function(a,b) {return(a+b)}");
            string expected =
@"function (a, b) {
    return (a + b)
}";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatInlineFunction()
        {
            RFormatter f = new RFormatter();
            string actual = f.Format("function(a,b) a+b");
            string expected = @"function (a, b) a + b";
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
@"x <- function (x,
 intercept = TRUE, tolerance = 1e-07,
	yname = NULL)
";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Formatter_FormatConditionalAlignBraces()
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
    }
}
