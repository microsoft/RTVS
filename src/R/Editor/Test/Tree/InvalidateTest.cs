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
    public class InvalidateTest
    {
        [TestMethod]
        public void EditorTree_InvalidateAll()
        {
            EditorTree tree = EditorTreeTest.MakeTree("if(true) x <- a + b");
            tree.Invalidate();

            Assert.AreEqual(0, tree.AstRoot.Children.Count);
        }

        [TestMethod]
        public void EditorTree_InvalidateInRangeTest()
        {
            EditorTree tree = EditorTreeTest.MakeTree("if(true) x <- a + b");

            bool nodesChanged = false;
            bool result = tree.InvalidateInRange(tree.AstRoot, new TextRange(4, 1), out nodesChanged);

            Assert.IsTrue(result);
            Assert.IsTrue(nodesChanged);
        }
    }
}
