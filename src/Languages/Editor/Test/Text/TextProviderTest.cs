using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Languages.Editor.Test.Text {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TextProviderTest {
        [TestMethod]
        public void TexProvider_GetCharAt() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            for (int i = 0; i < text.Length; i++) {
                Assert.AreEqual(text[i], textProvider[i]);
            }
        }

        [TestMethod()]
        public void TexProvider_GetText() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            Assert.AreEqual("wn fo", textProvider.GetText(9, 5));
        }

        [TestMethod()]
        public void TexProvider_IndexOf1() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            Assert.AreEqual(12, textProvider.IndexOf("fox", 0, true));
        }

        [TestMethod()]
        public void TexProvider_IndexOf2() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            Assert.AreEqual(8, textProvider.IndexOf("o", new TextRange(3, 7), true));
        }

        [TestMethod()]
        public void TexProvider_IndexOf_Range1() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            Assert.AreEqual(1, textProvider.IndexOf("uick", new TextRange(1, 4), true));
            Assert.AreEqual(1, textProvider.IndexOf("uick", new TextRange(1, 5), true));
            Assert.AreEqual(-1, textProvider.IndexOf("uick", new TextRange(1, 3), false));
            Assert.AreEqual(-1, textProvider.IndexOf("uick", new TextRange(1, 0), false));
        }

        [TestMethod()]
        public void TexProvider_CompareTo() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            Assert.IsTrue(textProvider.CompareTo(text.Length - 8, 4, "laZy", true));
            Assert.IsFalse(textProvider.CompareTo(text.Length - 8, 3, "laZy", true));
            Assert.IsFalse(textProvider.CompareTo(text.Length - 9, 4, "laZy", true));
            Assert.IsTrue(textProvider.CompareTo(text.Length - 3, 3, "dog", false));
            Assert.IsFalse(textProvider.CompareTo(text.Length - 3, 3, "dOg", false));
            Assert.IsFalse(textProvider.CompareTo(text.Length - 2, 3, "dog", true));
        }

        [TestMethod()]
        public void TexProvider_Boundary1() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            Assert.IsFalse(textProvider.CompareTo(text.Length, 4, "laZy", true));
            Assert.IsFalse(textProvider.CompareTo(text.Length - 1, 4, "laZy", true));
            Assert.IsFalse(textProvider.CompareTo(0, 3, new String('c', 100), true));
            Assert.IsTrue(textProvider.CompareTo(0, 0, "", true));
            Assert.IsFalse(textProvider.CompareTo(0, 2, String.Empty, true));
            Assert.IsFalse(textProvider.CompareTo(text.Length - 2, 3, "dog", true));
        }

        [TestMethod()]
        public void TexProvider_Boundary2() {
            var text = String.Empty;
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            Assert.IsFalse(textProvider.CompareTo(text.Length, 4, "laZy", true));
            Assert.IsFalse(textProvider.CompareTo(0, 3, new String('c', 100), true));
            Assert.IsTrue(textProvider.CompareTo(0, 0, "", true));
            Assert.IsFalse(textProvider.CompareTo(0, 2, String.Empty, true));
        }

        [TestMethod()]
        public void TexProvider_Boundary3() {
            var text = "ab";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            Assert.IsFalse(textProvider.CompareTo(text.Length, 4, "fooo", true));
            Assert.IsFalse(textProvider.CompareTo(0, 3, new String('c', 100), true));
            Assert.IsTrue(textProvider.CompareTo(0, 1, "a", true));
            Assert.IsTrue(textProvider.CompareTo(0, 2, "ab", true));
            Assert.IsFalse(textProvider.CompareTo(0, 2, "abc", true));
            Assert.IsTrue(textProvider.CompareTo(1, 0, String.Empty, true));
            Assert.IsTrue(textProvider.CompareTo(2, 0, String.Empty, true));
        }
    }
}
