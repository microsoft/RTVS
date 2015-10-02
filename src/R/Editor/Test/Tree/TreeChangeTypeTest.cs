using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Expressions.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Test.Tree
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TreeChangeTypeTest
    {
        [TestMethod]
        public void TextChange_EditWhitespaceTest01()
        {
            string expression = "x <- a + b";

            EditorTree tree = EditorTreeTest.ApplyTextChange(expression, 0, 0, 1, " ");
            Assert.AreEqual(TextChangeType.Trivial, tree.PendingChanges.TextChangeType);
        }

        [TestMethod]
        public void TextChange_EditWhitespaceTest02()
        {
            string expression = " x <- a + b";

            EditorTree tree = EditorTreeTest.ApplyTextChange(expression, 0, 1, 0, string.Empty);
            Assert.AreEqual(TextChangeType.Trivial, tree.PendingChanges.TextChangeType);
        }

        [TestMethod]
        public void TextChange_EditWhitespaceTest03()
        {
            string expression = "x <- a + b";

            EditorTree tree = EditorTreeTest.ApplyTextChange(expression, 1, 0, 1, "\n");
            Assert.AreEqual(TextChangeType.Trivial, tree.PendingChanges.TextChangeType);
        }

        [TestMethod]
        public void TextChange_EditString01()
        {
            string expression = "x <- a + \"boo\"";

            EditorTree tree = EditorTreeTest.ApplyTextChange(expression, 10, 1, 0, string.Empty);
            Assert.AreEqual(TextChangeType.Trivial, tree.PendingChanges.TextChangeType);
        }

        [TestMethod]
        public void TextChange_EditString02()
        {
            string expression = "x <- a + \"boo\"";

            EditorTree tree = EditorTreeTest.ApplyTextChange(expression, 10, 1, 2, "a");
            Assert.AreEqual(TextChangeType.Trivial, tree.PendingChanges.TextChangeType);
        }

        [TestMethod]
        public void TextChange_EditString03()
        {
            string expression = "x <- a + \"boo\"";

            EditorTree tree = EditorTreeTest.ApplyTextChange(expression, 10, 1, 2, "\"");
            Assert.AreEqual(TextChangeType.Structure, tree.PendingChanges.TextChangeType);
        }

        [TestMethod]
        public void TextChange_EditString04()
        {
            string expression = "\"boo\"";

            EditorTree tree = EditorTreeTest.ApplyTextChange(expression, 1, 0, 1, "a");
            Assert.AreEqual(TextChangeType.Trivial, tree.PendingChanges.TextChangeType);

            IScope scope = tree.AstRoot.Children[0] as IScope;
            Assert.AreEqual(1, scope.Children.Count);

            IStatement s = scope.Children[0] as IStatement;
            Assert.AreEqual(1, s.Children.Count);

            IExpression exp = s.Children[0] as IExpression;
            Assert.AreEqual(1, exp.Children.Count);

            TokenNode node = exp.Children[0] as TokenNode;
            Assert.AreEqual(RTokenType.String, node.Token.TokenType);
            Assert.AreEqual(0, node.Token.Start);
            Assert.AreEqual(6, node.Token.Length);
        }

        [TestMethod]
        public void TextChange_EditComment01()
        {
            string expression = "x <- a + b # comment";

            EditorTree tree = EditorTreeTest.ApplyTextChange(expression, 12, 1, 0, string.Empty);
            Assert.AreEqual(TextChangeType.Trivial, tree.PendingChanges.TextChangeType);
        }

        [TestMethod]
        public void TextChange_EditComment02()
        {
            string expression = "x <- a + b # comment";

            EditorTree tree = EditorTreeTest.ApplyTextChange(expression, 12, 1, 1, "a");
            Assert.AreEqual(TextChangeType.Trivial, tree.PendingChanges.TextChangeType);
        }

        [TestMethod]
        public void TextChange_EditComment03()
        {
            string expression = "x <- a + b # comment";

            EditorTree tree = EditorTreeTest.ApplyTextChange(expression, 12, 1, 2, "\n");
            Assert.AreEqual(TextChangeType.Structure, tree.PendingChanges.TextChangeType);
        }

        [TestMethod]
        public void TextChange_EditComment04()
        {
            string expression = "# comment\n a <- b + c";

            EditorTree tree = EditorTreeTest.ApplyTextChange(expression, 9, 1, 0, string.Empty);
            Assert.AreEqual(TextChangeType.Structure, tree.PendingChanges.TextChangeType);
        }

        [TestMethod]
        public void TextChange_EditComment05()
        {
            string expression = "#";

            EditorTree tree = EditorTreeTest.ApplyTextChange(expression, 1, 0, 1, "a");
            Assert.AreEqual(TextChangeType.Trivial, tree.PendingChanges.TextChangeType);

            Assert.AreEqual(1, tree.AstRoot.Comments.Count);
            Assert.AreEqual(0, tree.AstRoot.Comments[0].Start);
            Assert.AreEqual(2, tree.AstRoot.Comments[0].Length);
        }

        [TestMethod]
        public void TextChange_CurlyBrace()
        {
            string expression = "if(true) {x <- 1} else ";

            EditorTree tree = EditorTreeTest.ApplyTextChange(expression, expression.Length, 0, 1, "{");
            Assert.IsTrue(tree.IsDirty);
            Assert.AreEqual(TextChangeType.Structure, tree.PendingChanges.TextChangeType);
        }
    }
}
