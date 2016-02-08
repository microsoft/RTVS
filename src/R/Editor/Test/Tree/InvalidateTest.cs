using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Editor.Tree;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Editor.Test.Tree {
    [ExcludeFromCodeCoverage]
    [Category.R.EditorTree]
    public class InvalidateTest {
        [Test]
        public void EditorTree_InvalidateAll() {
            EditorTree tree = EditorTreeTest.MakeTree("if(true) x <- a + b");
            tree.Invalidate();
            tree.AstRoot.Children.Should().BeEmpty();
        }

        [Test]
        public void EditorTree_InvalidateInRangeTest() {
            EditorTree tree = EditorTreeTest.MakeTree("if(true) x <- a + b");

            bool nodesChanged = false;
            bool result = tree.InvalidateInRange(tree.AstRoot, new TextRange(4, 1), out nodesChanged);

            result.Should().BeTrue();
            nodesChanged.Should().BeTrue();
        }
    }
}
