using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    [TestClass]
    public class ParseAssignmentsTest : UnitTestBase
    {
        [TestMethod]
        public void ParseAssignmentsTest1()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x <- as.matrix(x)]
        Expression  [x <- as.matrix(x)]
            TokenOperator  [<- [2...4]]
                Variable  [x]
                TokenNode  [<- [2...4]]
                FunctionCall  [FunctionCall]
                    Variable  [as.matrix]
                    TokenNode  [( [14...15]]
                    ArgumentList  [ArgumentList]
                        ExpressionArgument  [ExpressionArgument]
                            Expression  [x]
                                Variable  [x]
                    TokenNode  [) [16...17]]
";
            ParserTest.VerifyParse(expected, "x <- as.matrix(x)");
        }

        [TestMethod]
        public void ParseAssignmentsTest2()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [as.matrix(x) -> x]
        Expression  [as.matrix(x) -> x]
            TokenOperator  [-> [13...15]]
                FunctionCall  [FunctionCall]
                    Variable  [as.matrix]
                    TokenNode  [( [9...10]]
                    ArgumentList  [ArgumentList]
                        ExpressionArgument  [ExpressionArgument]
                            Expression  [x]
                                Variable  [x]
                    TokenNode  [) [11...12]]
                TokenNode  [-> [13...15]]
                Variable  [x]
";
            ParserTest.VerifyParse(expected, "as.matrix(x) -> x");
        }

        [TestMethod]
        public void ParseAssignmentsTest3()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a <- b <- c <- 0]
        Expression  [a <- b <- c <- 0]
            TokenOperator  [<- [2...4]]
                Variable  [a]
                TokenNode  [<- [2...4]]
                TokenOperator  [<- [7...9]]
                    Variable  [b]
                    TokenNode  [<- [7...9]]
                    TokenOperator  [<- [12...14]]
                        Variable  [c]
                        TokenNode  [<- [12...14]]
                        NumericalValue  [0 [15...16]]
";
            ParserTest.VerifyParse(expected, "a <- b <- c <- 0");
        }

        [TestMethod]
        public void ParseAssignmentsTest4()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [0 -> a -> b]
        Expression  [0 -> a -> b]
            TokenOperator  [-> [7...9]]
                TokenOperator  [-> [2...4]]
                    NumericalValue  [0 [0...1]]
                    TokenNode  [-> [2...4]]
                    Variable  [a]
                TokenNode  [-> [7...9]]
                Variable  [b]
";
            ParserTest.VerifyParse(expected, "0 -> a -> b");
        }

        [TestMethod]
        public void ParseAssignmentsTest5()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [z <- .Call(x)]
        Expression  [z <- .Call(x)]
            TokenOperator  [<- [2...4]]
                Variable  [z]
                TokenNode  [<- [2...4]]
                FunctionCall  [FunctionCall]
                    Variable  [.Call]
                    TokenNode  [( [10...11]]
                    ArgumentList  [ArgumentList]
                        ExpressionArgument  [ExpressionArgument]
                            Expression  [x]
                                Variable  [x]
                    TokenNode  [) [12...13]]
";
            ParserTest.VerifyParse(expected, "z <- .Call(x)");
        }

        [TestMethod]
        public void ParseAssignmentsTest6()
        {
            string expected =
@"GlobalScope  [Global]

UnexpectedToken Token [0...2)
";
            ParserTest.VerifyParse(expected, "_z <- 0");
        }

        [TestMethod]
        public void ParseAssignmentsTest7()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [StudentData$ScoreRounded<-round(StudentData$Score)]
        Expression  [StudentData$ScoreRounded<-round(StudentData$Score)]
            TokenOperator  [<- [24...26]]
                TokenOperator  [$ [11...12]]
                    Variable  [StudentData]
                    TokenNode  [$ [11...12]]
                    Variable  [ScoreRounded]
                TokenNode  [<- [24...26]]
                FunctionCall  [FunctionCall]
                    Variable  [round]
                    TokenNode  [( [31...32]]
                    ArgumentList  [ArgumentList]
                        ExpressionArgument  [ExpressionArgument]
                            Expression  [StudentData$Score]
                                TokenOperator  [$ [43...44]]
                                    Variable  [StudentData]
                                    TokenNode  [$ [43...44]]
                                    Variable  [Score]
                    TokenNode  [) [49...50]]
";
            ParserTest.VerifyParse(expected, "StudentData$ScoreRounded<-round(StudentData$Score)");
        }
    }
}
