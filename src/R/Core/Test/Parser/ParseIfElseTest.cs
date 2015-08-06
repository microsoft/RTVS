using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    [TestClass]
    public class ParseIfElseTest : UnitTestBase
    {
        [TestMethod]
        public void ParseIfElseTest1()
        {
            string expected =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2]]
        TokenNode  [( [2...3]]
        Expression  [x < y]
            TokenOperator  [< [5...6]]
                Variable  [x]
                TokenNode  [< [5...6]]
                Variable  [y]
        TokenNode  [) [8...9]]
        SimpleScope  [SimpleScope]
            ExpressionStatement  [x <- x+1]
                Expression  [x <- x+1]
                    TokenOperator  [<- [12...14]]
                        Variable  [x]
                        TokenNode  [<- [12...14]]
                        TokenOperator  [+ [16...17]]
                            Variable  [x]
                            TokenNode  [+ [16...17]]
                            NumericalValue  [1 [17...18]]
";
            ParserTest.VerifyParse(expected, "if(x < y) x <- x+1");
        }

        [TestMethod]
        public void ParseIfElseTest2()
        {
            string expected =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2]]
        TokenNode  [( [2...3]]
        Expression  [x < y]
            TokenOperator  [< [5...6]]
                Variable  [x]
                TokenNode  [< [5...6]]
                Variable  [y]
        TokenNode  [) [8...9]]
        Scope  []
            TokenNode  [{ [10...11]]
            ExpressionStatement  [x <- x+1]
                Expression  [x <- x+1]
                    TokenOperator  [<- [14...16]]
                        Variable  [x]
                        TokenNode  [<- [14...16]]
                        TokenOperator  [+ [18...19]]
                            Variable  [x]
                            TokenNode  [+ [18...19]]
                            NumericalValue  [1 [19...20]]
            TokenNode  [} [21...22]]
";
            ParserTest.VerifyParse(expected, "if(x < y) { x <- x+1 }");
        }

        [TestMethod]
        public void ParseIfElseTest3()
        {
            string expected =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2]]
        TokenNode  [( [2...3]]
        Expression  [x < y]
            TokenOperator  [< [5...6]]
                Variable  [x]
                TokenNode  [< [5...6]]
                Variable  [y]
        TokenNode  [) [8...9]]
        Scope  []
            TokenNode  [{ [10...11]]
            ExpressionStatement  [x <- x+1]
                Expression  [x <- x+1]
                    TokenOperator  [<- [14...16]]
                        Variable  [x]
                        TokenNode  [<- [14...16]]
                        TokenOperator  [+ [18...19]]
                            Variable  [x]
                            TokenNode  [+ [18...19]]
                            NumericalValue  [1 [19...20]]
            TokenNode  [} [21...22]]
        KeywordScopeStatement  []
            TokenNode  [else [23...27]]
            Scope  []
                TokenNode  [{ [28...29]]
                ExpressionStatement  [x <- x + 2]
                    Expression  [x <- x + 2]
                        TokenOperator  [<- [32...34]]
                            Variable  [x]
                            TokenNode  [<- [32...34]]
                            TokenOperator  [+ [37...38]]
                                Variable  [x]
                                TokenNode  [+ [37...38]]
                                NumericalValue  [2 [39...40]]
                TokenNode  [} [41...42]]
";
            ParserTest.VerifyParse(expected, "if(x < y) { x <- x+1 } else { x <- x + 2 }");
        }

        [TestMethod]
        public void ParseIfElseTest4()
        {
            string expected =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2]]
        TokenNode  [( [2...3]]
        Expression  [x < y]
            TokenOperator  [< [5...6]]
                Variable  [x]
                TokenNode  [< [5...6]]
                Variable  [y]
        TokenNode  [) [8...9]]
        SimpleScope  [SimpleScope]
            ExpressionStatement  [x <- x+1]
                Expression  [x <- x+1]
                    TokenOperator  [<- [12...14]]
                        Variable  [x]
                        TokenNode  [<- [12...14]]
                        TokenOperator  [+ [16...17]]
                            Variable  [x]
                            TokenNode  [+ [16...17]]
                            NumericalValue  [1 [17...18]]
        KeywordScopeStatement  []
            TokenNode  [else [19...23]]
            Scope  []
                TokenNode  [{ [24...25]]
                ExpressionStatement  [x <- x + 2]
                    Expression  [x <- x + 2]
                        TokenOperator  [<- [28...30]]
                            Variable  [x]
                            TokenNode  [<- [28...30]]
                            TokenOperator  [+ [33...34]]
                                Variable  [x]
                                TokenNode  [+ [33...34]]
                                NumericalValue  [2 [35...36]]
                TokenNode  [} [37...38]]
";
            ParserTest.VerifyParse(expected, "if(x < y) x <- x+1 else { x <- x + 2 }");
        }

        [TestMethod]
        public void ParseIfElseTest5()
        {
            string expected =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2]]
        TokenNode  [( [2...3]]
        Expression  [x < y]
            TokenOperator  [< [5...6]]
                Variable  [x]
                TokenNode  [< [5...6]]
                Variable  [y]
        TokenNode  [) [8...9]]
        SimpleScope  [SimpleScope]
            ExpressionStatement  [x <- x+1]
                Expression  [x <- x+1]
                    TokenOperator  [<- [12...14]]
                        Variable  [x]
                        TokenNode  [<- [12...14]]
                        TokenOperator  [+ [16...17]]
                            Variable  [x]
                            TokenNode  [+ [16...17]]
                            NumericalValue  [1 [17...18]]
        KeywordScopeStatement  []
            TokenNode  [else [19...23]]
            SimpleScope  [SimpleScope]
                ExpressionStatement  [x <- x + 2]
                    Expression  [x <- x + 2]
                        TokenOperator  [<- [26...28]]
                            Variable  [x]
                            TokenNode  [<- [26...28]]
                            TokenOperator  [+ [31...32]]
                                Variable  [x]
                                TokenNode  [+ [31...32]]
                                NumericalValue  [2 [33...34]]
";
            ParserTest.VerifyParse(expected, "if(x < y) x <- x+1 else x <- x + 2");
        }

        [TestMethod]
        public void ParseIfElseTest6()
        {
            string expected =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2]]
        TokenNode  [( [2...3]]
        Expression  [x < y]
            TokenOperator  [< [5...6]]
                Variable  [x]
                TokenNode  [< [5...6]]
                Variable  [y]
        TokenNode  [) [8...9]]
        SimpleScope  [SimpleScope]
            ExpressionStatement  [x <- x+1]
                Expression  [x <- x+1]
                    TokenOperator  [<- [12...14]]
                        Variable  [x]
                        TokenNode  [<- [12...14]]
                        TokenOperator  [+ [16...17]]
                            Variable  [x]
                            TokenNode  [+ [16...17]]
                            NumericalValue  [1 [17...18]]
        KeywordScopeStatement  []
            TokenNode  [else [21...25]]
            SimpleScope  [SimpleScope]
                ExpressionStatement  [x <- x + 2]
                    Expression  [x <- x + 2]
                        TokenOperator  [<- [28...30]]
                            Variable  [x]
                            TokenNode  [<- [28...30]]
                            TokenOperator  [+ [33...34]]
                                Variable  [x]
                                TokenNode  [+ [33...34]]
                                NumericalValue  [2 [35...36]]
";
            ParserTest.VerifyParse(expected, "if(x < y) x <- x+1 \n else x <- x + 2");
        }

        [TestMethod]
        public void ParseIfElseTest7()
        {
            string expected =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2]]
        TokenNode  [( [2...3]]
        Expression  [x < y]
            TokenOperator  [< [5...6]]
                Variable  [x]
                TokenNode  [< [5...6]]
                Variable  [y]
        TokenNode  [) [8...9]]
        SimpleScope  [SimpleScope]
            ExpressionStatement  [x <- x+1]
                Expression  [x <- x+1]
                    TokenOperator  [<- [14...16]]
                        Variable  [x]
                        TokenNode  [<- [14...16]]
                        TokenOperator  [+ [18...19]]
                            Variable  [x]
                            TokenNode  [+ [18...19]]
                            NumericalValue  [1 [19...20]]
        KeywordScopeStatement  []
            TokenNode  [else [21...25]]
            Scope  []
                TokenNode  [{ [28...29]]
                ExpressionStatement  [x <- x + 2]
                    Expression  [x <- x + 2]
                        TokenOperator  [<- [32...34]]
                            Variable  [x]
                            TokenNode  [<- [32...34]]
                            TokenOperator  [+ [37...38]]
                                Variable  [x]
                                TokenNode  [+ [37...38]]
                                NumericalValue  [2 [39...40]]
                TokenNode  [} [41...42]]
";
            ParserTest.VerifyParse(expected, "if(x < y) \n x <- x+1 else \n { x <- x + 2 }");
        }

        [TestMethod]
        public void ParseIfElseTest8()
        {
            string expected =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2]]
        TokenNode  [( [2...3]]
        Expression  [x < y]
            TokenOperator  [< [5...6]]
                Variable  [x]
                TokenNode  [< [5...6]]
                Variable  [y]
        TokenNode  [) [8...9]]
        Scope  []
            TokenNode  [{ [10...11]]
            ExpressionStatement  [x <- x+1]
                Expression  [x <- x+1]
                    TokenOperator  [<- [14...16]]
                        Variable  [x]
                        TokenNode  [<- [14...16]]
                        TokenOperator  [+ [18...19]]
                            Variable  [x]
                            TokenNode  [+ [18...19]]
                            NumericalValue  [1 [19...20]]
            TokenNode  [} [21...22]]
        KeywordScopeStatement  []
            TokenNode  [else [25...29]]
            SimpleScope  [SimpleScope]
                ExpressionStatement  [x <- x + 2]
                    Expression  [x <- x + 2]
                        TokenOperator  [<- [34...36]]
                            Variable  [x]
                            TokenNode  [<- [34...36]]
                            TokenOperator  [+ [39...40]]
                                Variable  [x]
                                TokenNode  [+ [39...40]]
                                NumericalValue  [2 [41...42]]
";
            ParserTest.VerifyParse(expected, "if(x < y) { x <- x+1 } \n else \n x <- x + 2");
        }
    }
}
