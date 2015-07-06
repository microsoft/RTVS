using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens
{
    [TestClass]
    public class ParseIndexerTest : TokenizeTestBase
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
                IndexerArgumentList  [IndexerArgumentList]
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
                IndexerArgumentList  [IndexerArgumentList]
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
    ExpressionStatement  [x(a)]
        Expression  [x(a)]
            FunctionCall  [FunctionCall]
                Indexer  [Indexer]
                    Variable  [x]
                    TokenNode  [[ [4...5]]
                    IndexerArgumentList  [IndexerArgumentList]
                        ExpressionArgument  [ExpressionArgument]
                            Expression  [1]
                                NumericalValue  [1 [5...6]]
                    TokenNode  [] [6...7]]
                TokenNode  [( [1...2]]
                FunctionArgumentList  [FunctionArgumentList]
                    NamedArgument  [NamedArgument]
                        Expression  [a]
                            Variable  [a]
                TokenNode  [) [3...4]]
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
    }
}
