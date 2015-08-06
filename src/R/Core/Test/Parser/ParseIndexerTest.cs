using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    [TestClass]
    public class ParseIndexerTest : UnitTestBase
    {
        [TestMethod]
        public void ParseIndexerTest1()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [a[1]]
        Expression  [a[1]]
            Indexer  [Indexer]
                Variable  [a]
                TokenNode  [[ [1...2]]
                ArgumentList  [ArgumentList]
                    ExpressionArgument  [ExpressionArgument]
                        Expression  [1]
                            NumericalValue  [1 [2...3]]
                TokenNode  [] [3...4]]
";
            ParserTest.VerifyParse(expected, "a[1]");
        }

        [TestMethod]
        public void ParseIndexerTest2()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[1,2]]
        Expression  [x[1,2]]
            Indexer  [Indexer]
                Variable  [x]
                TokenNode  [[ [1...2]]
                ArgumentList  [ArgumentList]
                    ExpressionArgument  [ExpressionArgument]
                        Expression  [1]
                            NumericalValue  [1 [2...3]]
                        TokenNode  [, [3...4]]
                    ExpressionArgument  [ExpressionArgument]
                        Expression  [2]
                            NumericalValue  [2 [4...5]]
                TokenNode  [] [5...6]]
";
            ParserTest.VerifyParse(expected, "x[1,2]");
        }

        [TestMethod]
        public void ParseIndexerTest3()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x(a)[1]]
        Expression  [x(a)[1]]
            Indexer  [Indexer]
                FunctionCall  [FunctionCall]
                    Variable  [x]
                    TokenNode  [( [1...2]]
                    ArgumentList  [ArgumentList]
                        ExpressionArgument  [ExpressionArgument]
                            Expression  [a]
                                Variable  [a]
                    TokenNode  [) [3...4]]
                TokenNode  [[ [4...5]]
                ArgumentList  [ArgumentList]
                    ExpressionArgument  [ExpressionArgument]
                        Expression  [1]
                            NumericalValue  [1 [5...6]]
                TokenNode  [] [6...7]]
";
            ParserTest.VerifyParse(expected, "x(a)[1]");
        }

        [TestMethod]
        public void ParseIndexerTest4()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[[a+(b*c)]]]
        Expression  [x[[a+(b*c)]]]
            Indexer  [Indexer]
                Variable  [x]
                TokenNode  [[[ [1...3]]
                ArgumentList  [ArgumentList]
                    ExpressionArgument  [ExpressionArgument]
                        Expression  [a+(b*c)]
                            TokenOperator  [+ [4...5]]
                                Variable  [a]
                                TokenNode  [+ [4...5]]
                                Expression  [(b*c)]
                                    TokenNode  [( [5...6]]
                                    TokenOperator  [* [7...8]]
                                        Variable  [b]
                                        TokenNode  [* [7...8]]
                                        Variable  [c]
                                    TokenNode  [) [9...10]]
                TokenNode  []] [10...12]]
";
            ParserTest.VerifyParse(expected, "x[[a+(b*c)]]");
        }

        [TestMethod]
        public void ParseIndexerTest5()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[1](c)]
        Expression  [x[1](c)]
            FunctionCall  [FunctionCall]
                Indexer  [Indexer]
                    Variable  [x]
                    TokenNode  [[ [1...2]]
                    ArgumentList  [ArgumentList]
                        ExpressionArgument  [ExpressionArgument]
                            Expression  [1]
                                NumericalValue  [1 [2...3]]
                    TokenNode  [] [3...4]]
                TokenNode  [( [4...5]]
                ArgumentList  [ArgumentList]
                    ExpressionArgument  [ExpressionArgument]
                        Expression  [c]
                            Variable  [c]
                TokenNode  [) [6...7]]
";
            ParserTest.VerifyParse(expected, "x[1](c)");
        }

        [TestMethod]
        public void ParseIndexerTest6()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[, 1]]
        Expression  [x[, 1]]
            Indexer  [Indexer]
                Variable  [x]
                TokenNode  [[ [1...2]]
                ArgumentList  [ArgumentList]
                    MissingArgument  [{Missing}]
                        TokenNode  [, [2...3]]
                    ExpressionArgument  [ExpressionArgument]
                        Expression  [1]
                            NumericalValue  [1 [4...5]]
                TokenNode  [] [5...6]]
";
            ParserTest.VerifyParse(expected, "x[, 1]");
        }

        [TestMethod]
        public void ParseIndexerTest7()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[2,]]
        Expression  [x[2,]]
            Indexer  [Indexer]
                Variable  [x]
                TokenNode  [[ [1...2]]
                ArgumentList  [ArgumentList]
                    ExpressionArgument  [ExpressionArgument]
                        Expression  [2]
                            NumericalValue  [2 [2...3]]
                        TokenNode  [, [3...4]]
                TokenNode  [] [4...5]]
";
            ParserTest.VerifyParse(expected, "x[2,]");
        }

        [TestMethod]
        public void ParseIndexerTest8()
        {
            string expected =
@"GlobalScope  [Global]
    ExpressionStatement  [x[,,]]
        Expression  [x[,,]]
            Indexer  [Indexer]
                Variable  [x]
                TokenNode  [[ [1...2]]
                ArgumentList  [ArgumentList]
                    MissingArgument  [{Missing}]
                        TokenNode  [, [2...3]]
                    MissingArgument  [{Missing}]
                        TokenNode  [, [3...4]]
                TokenNode  [] [4...5]]
";
            ParserTest.VerifyParse(expected, "x[,,]");
        }

        [TestMethod]
        public void ParseIndexerTest9()
        {
            string expected =
@"GlobalScope  [Global]
";
            ParserTest.VerifyParse(expected, "");
        }

        [TestMethod]
        public void ParseIndexerTest10()
        {
            string expected =
"GlobalScope  [Global]\r\n" +
"    ExpressionStatement  [colnames(data)[colnames(data)==\"old_name\"] <- \"new_name\"]\r\n"+
"        Expression  [colnames(data)[colnames(data)==\"old_name\"] <- \"new_name\"]\r\n" +
"            TokenOperator  [<- [43...45]]\r\n" +
"                Indexer  [Indexer]\r\n" +
"                    FunctionCall  [FunctionCall]\r\n" +
"                        Variable  [colnames]\r\n" +
"                        TokenNode  [( [8...9]]\r\n" +
"                        ArgumentList  [ArgumentList]\r\n" +
"                            ExpressionArgument  [ExpressionArgument]\r\n" +
"                                Expression  [data]\r\n" +
"                                    Variable  [data]\r\n" +
"                        TokenNode  [) [13...14]]\r\n" +
"                    TokenNode  [[ [14...15]]\r\n" +
"                    ArgumentList  [ArgumentList]\r\n" +
"                        ExpressionArgument  [ExpressionArgument]\r\n" +
"                            Expression  [colnames(data)==\"old_name\"]\r\n" +
"                                TokenOperator  [== [29...31]]\r\n" +
"                                    FunctionCall  [FunctionCall]\r\n" +
"                                        Variable  [colnames]\r\n" +
"                                        TokenNode  [( [23...24]]\r\n" +
"                                        ArgumentList  [ArgumentList]\r\n" +
"                                            ExpressionArgument  [ExpressionArgument]\r\n" +
"                                                Expression  [data]\r\n" +
"                                                    Variable  [data]\r\n" +
"                                        TokenNode  [) [28...29]]\r\n" +
"                                    TokenNode  [== [29...31]]\r\n" +
"                                    StringValue  [\"old_name\" [31...41]]\r\n" +
"                    TokenNode  [] [41...42]]\r\n" +
"                TokenNode  [<- [43...45]]\r\n" +
"                StringValue  [\"new_name\" [46...56]]\r\n";

            ParserTest.VerifyParse(expected, "colnames(data)[colnames(data)==\"old_name\"] <- \"new_name\"");
        }
    }
}
