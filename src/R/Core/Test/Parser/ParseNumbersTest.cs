using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class ParseNumbersTest {
        [Test]
        [Category.R.Parser]
        public void ParseNumbers01() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [1e4L]
        Expression  [1e4L]
            NumericalValue  [1e4L [0...4)]
";
            ParserTest.VerifyParse(expected, "1e4L");
        }

        [Test]
        [Category.R.Parser]
        public void ParseNumbers02() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [3600L]
        Expression  [3600L]
            NumericalValue  [3600L [0...5)]
";
            ParserTest.VerifyParse(expected, "3600L");
        }

        [Test]
        [Category.R.Parser]
        public void ParseNumbers03() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [2.4L]
        Expression  [2.4L]
            NumericalValue  [2.4L [0...4)]
";
            ParserTest.VerifyParse(expected, "2.4L");
        }

        [Test]
        [Category.R.Parser]
        public void ParseNumbers04() {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [3600L + x/1.0e+5L]
        Expression  [3600L + x/1.0e+5L]
            TokenOperator  [+ [6...7)]
                NumericalValue  [3600L [0...5)]
                TokenNode  [+ [6...7)]
                TokenOperator  [/ [9...10)]
                    Variable  [x]
                    TokenNode  [/ [9...10)]
                    NumericalValue  [1.0e+5L [10...17)]
";
            ParserTest.VerifyParse(expected, "3600L + x/1.0e+5L");
        }
    }
}
