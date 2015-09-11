using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Test.Mocks;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.Test.Utility;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using TextChange = Microsoft.R.Editor.Tree.TextChange;

namespace Microsoft.R.Editor.Test.Tree
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ProcessChangesTest
    {
        [TestMethod]
        public void ProcessChange_EditExpression01()
        {
            string expression = "if(true) x <- 1";
            string expected1 =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2)]
        TokenNode  [( [2...3)]
        Expression  [true]
            Variable  [true]
        TokenNode  [) [7...8)]
        SimpleScope  [9...15)
            ExpressionStatement  [x <- 1]
                Expression  [x <- 1]
                    TokenOperator  [<- [11...13)]
                        Variable  [x]
                        TokenNode  [<- [11...13)]
                        NumericalValue  [1 [14...15)]
";
            ParserTest.VerifyParse(expected1, expression);

            EditorTree tree = EditorTreeTest.ApplyTextChange(expression, 3, 4, 5, "false");
            Assert.IsTrue(tree.IsDirty);
            tree.ProcessChanges();

            string expected2 =
@"GlobalScope  [Global]
    If  []
        TokenNode  [if [0...2)]
        TokenNode  [( [2...3)]
        Expression  [false]
            Variable  [false]
        TokenNode  [) [8...9)]
        SimpleScope  [10...16)
            ExpressionStatement  [x <- 1]
                Expression  [x <- 1]
                    TokenOperator  [<- [12...14)]
                        Variable  [x]
                        TokenNode  [<- [12...14)]
                        NumericalValue  [1 [15...16)]
";
            ParserTest.CompareTrees(expected2, tree.AstRoot);
        }
    }
}
