using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Test.Mocks;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using TextChange = Microsoft.R.Editor.Tree.TextChange;

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
        public void TextChange_CurlyBrace()
        {
            string expression = "if(true) {x <- 1} else ";

            EditorTree tree = EditorTreeTest.ApplyTextChange(expression, expression.Length, 0, 1, "{");
            Assert.IsTrue(tree.IsDirty);
            Assert.AreEqual(TextChangeType.Structure, tree.PendingChanges.TextChangeType);
        }
    }
}
