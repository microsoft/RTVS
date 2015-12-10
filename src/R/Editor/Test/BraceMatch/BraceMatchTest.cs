using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.EditorHelpers;
using Microsoft.R.Editor.BraceMatch;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Test.Text {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RBraceMatchTest {
        [TestMethod]
        [TestCategory("R.BraceMatch")]
        public void RBraceMatch_CurlyBraces01() {
            ITextView tv = TextViewTest.MakeTextViewRealTextBuffer("a{\"{ }\"}b");
            RBraceMatcher bm = new RBraceMatcher(tv, tv.TextBuffer);

            int startPosition, endPosition;
            bool result;

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 0, false, out startPosition, out endPosition);
            Assert.IsFalse(result);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 1, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(1, startPosition);
            Assert.AreEqual(7, endPosition);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 2, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(1, startPosition);
            Assert.AreEqual(7, endPosition);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 3, false, out startPosition, out endPosition);
            Assert.IsFalse(result);
        }

        [TestMethod]
        [TestCategory("R.BraceMatch")]
        public void RBraceMatch_CurlyBraces02() {
            ITextView tv = TextViewTest.MakeTextViewRealTextBuffer("{{\"{ }\"}}");
            RBraceMatcher bm = new RBraceMatcher(tv, tv.TextBuffer);

            int startPosition, endPosition;
            bool result;

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 0, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(0, startPosition);
            Assert.AreEqual(8, endPosition);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 1, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(1, startPosition);
            Assert.AreEqual(7, endPosition);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 3, false, out startPosition, out endPosition);
            Assert.IsFalse(result);
        }

        //[TestMethod]
        [TestCategory("R.BraceMatch")]
        public void RBraceMatch_MixedBraces() {
            ITextView tv = TextViewTest.MakeTextViewRealTextBuffer("[[{()}]]");
            RBraceMatcher bm = new RBraceMatcher(tv, tv.TextBuffer);

            int startPosition, endPosition;
            bool result;

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 0, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(0, startPosition);
            Assert.AreEqual(7, endPosition);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 1, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(1, startPosition);
            Assert.AreEqual(6, endPosition);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 2, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(2, startPosition);
            Assert.AreEqual(5, endPosition);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 3, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(3, startPosition);
            Assert.AreEqual(4, endPosition);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 4, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(3, startPosition);
            Assert.AreEqual(4, endPosition);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 5, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(2, startPosition);
            Assert.AreEqual(5, endPosition);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 6, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(1, startPosition);
            Assert.AreEqual(6, endPosition);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 7, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(0, startPosition);
            Assert.AreEqual(7, endPosition);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 8, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(0, startPosition);
            Assert.AreEqual(7, endPosition);
        }

        [TestMethod]
        [TestCategory("R.BraceMatch")]
        public void RBraceMatch_Braces() {
            ITextView tv = TextViewTest.MakeTextViewRealTextBuffer("a(\"( )\")b");
            RBraceMatcher bm = new RBraceMatcher(tv, tv.TextBuffer);

            int startPosition, endPosition;
            bool result;

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 0, false, out startPosition, out endPosition);
            Assert.IsFalse(result);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 1, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(1, startPosition);
            Assert.AreEqual(7, endPosition);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 2, false, out startPosition, out endPosition);
            Assert.IsTrue(result);
            Assert.AreEqual(1, startPosition);
            Assert.AreEqual(7, endPosition);

            result = bm.GetBracesFromPosition(tv.TextBuffer.CurrentSnapshot, 3, false, out startPosition, out endPosition);
            Assert.IsFalse(result);
        }
    }
}
