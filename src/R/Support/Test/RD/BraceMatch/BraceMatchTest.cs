using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Support.RD.BraceMatch;
using Microsoft.R.Support.RD.ContentTypes;
using Microsoft.VisualStudio.Editor.Mocks.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Support.Test.RD.BraceMatch {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RBraceMatchTest {
        [TestMethod]
        [TestCategory("Rd.BraceMatch")]
        public void RdBraceMatch_MixedBraces() {
            ITextBuffer textBuffer;
            RdBraceMatcher bm = CreateBraceMatcher("\\latex[0]{foo} \\item{}{}", out textBuffer);

            int startPosition, endPosition;
            bool result;

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 0, false, out startPosition, out endPosition);
            Assert.IsFalse(result);

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 6, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(6, startPosition);
            Assert.AreEqual(8, endPosition);

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 7, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(6, startPosition);
            Assert.AreEqual(8, endPosition);

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 9, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(9, startPosition);
            Assert.AreEqual(13, endPosition);

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 13, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(9, startPosition);
            Assert.AreEqual(13, endPosition);

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 14, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(9, startPosition);
            Assert.AreEqual(13, endPosition);
        }

        [TestMethod]
        [TestCategory("Rd.BraceMatch")]
        public void RdBraceMatch_SquareBrackets() {
            ITextBuffer textBuffer;
            RdBraceMatcher bm = CreateBraceMatcher("\\a[[b]]", out textBuffer);

            int startPosition, endPosition;
            bool result;

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 1, false, out startPosition, out endPosition);
            Assert.IsFalse(result);

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 2, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(2, startPosition);
            Assert.AreEqual(6, endPosition);

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 3, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(3, startPosition);
            Assert.AreEqual(5, endPosition);

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 5, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(3, startPosition);
            Assert.AreEqual(5, endPosition);

            result = bm.GetBracesFromPosition(textBuffer.CurrentSnapshot, 6, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(2, startPosition);
            Assert.AreEqual(6, endPosition);
        }

        private RdBraceMatcher CreateBraceMatcher(string content, out ITextBuffer textBuffer) {
            ITextView tv = TextViewTestHelper.MakeTextView(content, RdContentTypeDefinition.ContentType, TextRange.EmptyRange);
            textBuffer = tv.TextBuffer;
            return new RdBraceMatcher(tv, tv.TextBuffer);
        }
    }
}
