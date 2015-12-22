using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Common.Core.Tests.Utility;
using Microsoft.Languages.Editor.Outline;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Outline;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Test.Outline {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class OutlineBuilderTest : UnitTestBase {
        [TestMethod]
        [TestCategory("R.Outlining")]
        public void RRegionBuilder_ConstructionTest() {
            TextBufferMock textBuffer = new TextBufferMock(string.Empty, RContentTypeDefinition.ContentType);
            EditorTree tree = new EditorTree(textBuffer);
            EditorDocumentMock editorDocument = new EditorDocumentMock(tree);

            ROutlineRegionBuilder ob = new ROutlineRegionBuilder(editorDocument);

            Assert.IsNotNull(ob.EditorDocument);
            Assert.IsNotNull(ob.EditorTree);

            FieldInfo docCloseField = editorDocument.GetType().GetField("DocumentClosing", BindingFlags.Instance | BindingFlags.NonPublic);
            MulticastDelegate d = (MulticastDelegate)docCloseField.GetValue(editorDocument);
            Assert.AreEqual(1, d.GetInvocationList().Length);

            FieldInfo treeUpdateField = tree.GetType().GetField("UpdateCompleted", BindingFlags.Instance | BindingFlags.NonPublic);
            d = (MulticastDelegate)treeUpdateField.GetValue(tree);
            Assert.AreEqual(1, d.GetInvocationList().Length);

            ob.Dispose();

            d = (MulticastDelegate)docCloseField.GetValue(editorDocument);
            Assert.IsNull(d);

            d = (MulticastDelegate)treeUpdateField.GetValue(tree);
            Assert.IsNull(d);
        }

        [TestMethod]
        [TestCategory("R.Outlining")]
        public void RRegionBuilder_Test01() {
            OutlineRegionCollection rc = OutlineTest.BuildOutlineRegions("");

            Assert.AreEqual(0, rc.Count);
            Assert.AreEqual(0, rc.Start);
            Assert.AreEqual(0, rc.Length);
        }

        [TestMethod]
        [TestCategory("R.Outlining")]
        public void RRegionBuilder_Test02() {
            string content =
@"if (ncol(x) == 1L) {
    xnames < -1
}
else {
    xnames < -paste0(1, 1L:ncol(x))
  }
  if (intercept) {
    x<- cbind(1, x)
    xnames<- c(0, xnames)
  }
";
            OutlineRegionCollection rc = OutlineTest.BuildOutlineRegions(content);

            // [0][0...165), Length = 165
            // [1][42...90), Length = 48
            // [2][94...163), Length = 69
            Assert.AreEqual(3, rc.Count);

            Assert.AreEqual(0, rc[0].Start);
            Assert.AreEqual(90, rc[0].Length);

            Assert.AreEqual(42, rc[1].Start);
            Assert.AreEqual(90, rc[1].End);
            Assert.AreEqual("else...", rc[1].DisplayText);

            Assert.AreEqual(94, rc[2].Start);
            Assert.AreEqual(163, rc[2].End);
            Assert.AreEqual("if...", rc[2].DisplayText);
        }

        [TestMethod]
        [TestCategory("R.Outlining")]
        public void RRegionBuilder_OutlineFile01() {
            OutlineTest.OutlineFile(this.TestContext, "01.r");
        }
    }
}
