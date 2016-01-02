using System;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Xunit;

namespace Microsoft.Languages.Editor.Tests.Text {
    public class TextProviderTest {
        [Fact]
        [Trait("Category", "Languages.Editor")]
        public void TextProvider_GetCharAt() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            for (int i = 0; i < text.Length; i++) {
                Assert.Equal(text[i], textProvider[i]);
            }
        }

        [Fact]
        [Trait("Category", "Languages.Editor")]
        public void TextProvider_GetText() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            Assert.Equal("wn fo", textProvider.GetText(9, 5));
        }

        [Fact]
        [Trait("Category", "Languages.Editor")]
        public void TextProvider_IndexOf1() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            Assert.Equal(12, textProvider.IndexOf("fox", 0, true));
        }

        [Fact]
        [Trait("Category", "Languages.Editor")]
        public void TextProvider_IndexOf2() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            Assert.Equal(8, textProvider.IndexOf("o", new TextRange(3, 7), true));
        }

        [Fact]
        [Trait("Category", "Languages.Editor")]
        public void TextProvider_IndexOf_Range1() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            Assert.Equal(1, textProvider.IndexOf("uick", new TextRange(1, 4), true));
            Assert.Equal(1, textProvider.IndexOf("uick", new TextRange(1, 5), true));
            Assert.Equal(-1, textProvider.IndexOf("uick", new TextRange(1, 3), false));
            Assert.Equal(-1, textProvider.IndexOf("uick", new TextRange(1, 0), false));
        }

        [Fact]
        [Trait("Category", "Languages.Editor")]
        public void TextProvider_CompareTo() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            Assert.True(textProvider.CompareTo(text.Length - 8, 4, "laZy", true));
            Assert.False(textProvider.CompareTo(text.Length - 8, 3, "laZy", true));
            Assert.False(textProvider.CompareTo(text.Length - 9, 4, "laZy", true));
            Assert.True(textProvider.CompareTo(text.Length - 3, 3, "dog", false));
            Assert.False(textProvider.CompareTo(text.Length - 3, 3, "dOg", false));
            Assert.False(textProvider.CompareTo(text.Length - 2, 3, "dog", true));
        }

        [Fact]
        [Trait("Category", "Languages.Editor")]
        public void TextProvider_Boundary1() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            Assert.False(textProvider.CompareTo(text.Length, 4, "laZy", true));
            Assert.False(textProvider.CompareTo(text.Length - 1, 4, "laZy", true));
            Assert.False(textProvider.CompareTo(0, 3, new String('c', 100), true));
            Assert.True(textProvider.CompareTo(0, 0, "", true));
            Assert.False(textProvider.CompareTo(0, 2, String.Empty, true));
            Assert.False(textProvider.CompareTo(text.Length - 2, 3, "dog", true));
        }

        [Fact]
        [Trait("Category", "Languages.Editor")]
        public void TextProvider_Boundary2() {
            var text = String.Empty;
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            Assert.False(textProvider.CompareTo(text.Length, 4, "laZy", true));
            Assert.False(textProvider.CompareTo(0, 3, new String('c', 100), true));
            Assert.True(textProvider.CompareTo(0, 0, "", true));
            Assert.False(textProvider.CompareTo(0, 2, String.Empty, true));
        }

        [Fact]
        [Trait("Category", "Languages.Editor")]
        public void TextProvider_Boundary3() {
            var text = "ab";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            Assert.False(textProvider.CompareTo(text.Length, 4, "fooo", true));
            Assert.False(textProvider.CompareTo(0, 3, new String('c', 100), true));
            Assert.True(textProvider.CompareTo(0, 1, "a", true));
            Assert.True(textProvider.CompareTo(0, 2, "ab", true));
            Assert.False(textProvider.CompareTo(0, 2, "abc", true));
            Assert.True(textProvider.CompareTo(1, 0, String.Empty, true));
            Assert.True(textProvider.CompareTo(2, 0, String.Empty, true));
        }
    }
}
