using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ParseFunctionsTest : UnitTestBase
    {
        [TestMethod]
        public void ParseFunctionsTest1()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a()]
        Expression  [a()]
            FunctionCall  [0...3)
                Variable  [a]
                TokenNode  [( [1...2)]
                TokenNode  [) [2...3)]
";
            ParserTest.VerifyParse(expected, "a()");
        }

        [TestMethod]
        public void ParseFunctionsTest2()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a(1)]
        Expression  [a(1)]
            FunctionCall  [0...4)
                Variable  [a]
                TokenNode  [( [1...2)]
                ArgumentList  [2...3)
                    ExpressionArgument  [2...3)
                        Expression  [1]
                            NumericalValue  [1 [2...3)]
                TokenNode  [) [3...4)]
";
            ParserTest.VerifyParse(expected, "a(1)");
        }

        [TestMethod]
        public void ParseFunctionsTest3()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a(1,2)]
        Expression  [a(1,2)]
            FunctionCall  [0...6)
                Variable  [a]
                TokenNode  [( [1...2)]
                ArgumentList  [2...5)
                    ExpressionArgument  [2...4)
                        Expression  [1]
                            NumericalValue  [1 [2...3)]
                        TokenNode  [, [3...4)]
                    ExpressionArgument  [4...5)
                        Expression  [2]
                            NumericalValue  [2 [4...5)]
                TokenNode  [) [5...6)]
";
            ParserTest.VerifyParse(expected, "a(1,2)");
        }

        [TestMethod]
        public void ParseFunctionsTest4()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x(a, b=NA, c=NULL, ...)]
        Expression  [x(a, b=NA, c=NULL, ...)]
            FunctionCall  [0...23)
                Variable  [x]
                TokenNode  [( [1...2)]
                ArgumentList  [2...22)
                    ExpressionArgument  [2...4)
                        Expression  [a]
                            Variable  [a]
                        TokenNode  [, [3...4)]
                    NamedArgument  [5...10)
                        TokenNode  [b [5...6)]
                        TokenNode  [= [6...7)]
                        Expression  [NA]
                            MissingValue  [NA [7...9)]
                        TokenNode  [, [9...10)]
                    NamedArgument  [11...18)
                        TokenNode  [c [11...12)]
                        TokenNode  [= [12...13)]
                        Expression  [NULL]
                            NullValue  [NULL [13...17)]
                        TokenNode  [, [17...18)]
                    EllipsisArgument  [...]
                        TokenNode  [... [19...22)]
                TokenNode  [) [22...23)]
";
            ParserTest.VerifyParse(expected, "x(a, b=NA, c=NULL, ...)");
        }

        [TestMethod]
        public void ParseFunctionsTest5()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x(,, ]
        Expression  [x(,, ]
            FunctionCall  [0...5)
                Variable  [x]
                TokenNode  [( [1...2)]
                ArgumentList  [2...4)
                    MissingArgument  [{Missing}]
                        TokenNode  [, [2...3)]
                    MissingArgument  [{Missing}]
                        TokenNode  [, [3...4)]
                    StubArgument  [{Stub}]

CloseBraceExpected AfterToken [3...4)
";
            ParserTest.VerifyParse(expected, "x(,, ");
        }

        [TestMethod]
        public void ParseFunctionsTest6()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x(,, ]
        Expression  [x(,, ]
            FunctionCall  [0...5)
                Variable  [x]
                TokenNode  [( [1...2)]
                ArgumentList  [2...4)
                    MissingArgument  [{Missing}]
                        TokenNode  [, [2...3)]
                    MissingArgument  [{Missing}]
                        TokenNode  [, [3...4)]
                    StubArgument  [{Stub}]

CloseBraceExpected AfterToken [3...4)
UnexpectedToken AfterToken [3...4)
UnexpectedToken Token [5...8)
OpenBraceExpected AfterToken [5...8)";
            ParserTest.VerifyParse(expected, "x(,, for");
        }
    }
}
