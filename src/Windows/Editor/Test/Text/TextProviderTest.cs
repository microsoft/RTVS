// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;

namespace Microsoft.Languages.Editor.Test.Text {
    [ExcludeFromCodeCoverage]
    public class TextProviderTest {
        [Test]
        [Category.Languages.Core]
        public void TextProvider_GetCharAt() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            for (int i = 0; i < text.Length; i++) {
                textProvider[i].Should().Be(text[i]);
            }
        }

        [Test]
        [Category.Languages.Core]
        public void TextProvider_GetText() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            textProvider.GetText(9, 5).Should().Be("wn fo");
        }

        [Test]
        [Category.Languages.Core]
        public void TextProvider_IndexOf1() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            textProvider.IndexOf("fox", 0, true).Should().Be(12);
        }

        [Test]
        [Category.Languages.Core]
        public void TextProvider_IndexOf2() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            textProvider.IndexOf("o", new TextRange(3, 7), true).Should().Be(8);
        }

        [Test]
        [Category.Languages.Core]
        public void TextProvider_IndexOf_Range1() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            textProvider.IndexOf("uick", new TextRange(1, 4), true).Should().Be(1);
            textProvider.IndexOf("uick", new TextRange(1, 5), true).Should().Be(1);
            textProvider.IndexOf("uick", new TextRange(1, 3), false).Should().Be(-1);
            textProvider.IndexOf("uick", new TextRange(1, 0), false).Should().Be(-1);
        }

        [Test]
        [Category.Languages.Core]
        public void TextProvider_CompareTo() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            textProvider.CompareTo(text.Length - 8, 4, "laZy", true).Should().BeTrue();
            textProvider.CompareTo(text.Length - 8, 3, "laZy", true).Should().BeFalse();
            textProvider.CompareTo(text.Length - 9, 4, "laZy", true).Should().BeFalse();
            textProvider.CompareTo(text.Length - 3, 3, "dog", false).Should().BeTrue();
            textProvider.CompareTo(text.Length - 3, 3, "dOg", false).Should().BeFalse();
            textProvider.CompareTo(text.Length - 2, 3, "dog", true).Should().BeFalse();
        }

        [Test]
        [Category.Languages.Core]
        public void TextProvider_Boundary1() {
            var text = "Quick brown fox jumps over the lazy dog";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            textProvider.CompareTo(text.Length, 4, "laZy", true).Should().BeFalse();
            textProvider.CompareTo(text.Length - 1, 4, "laZy", true).Should().BeFalse();
            textProvider.CompareTo(0, 3, new string('c', 100), true).Should().BeFalse();
            textProvider.CompareTo(0, 0, "", true).Should().BeTrue();
            textProvider.CompareTo(0, 2, string.Empty, true).Should().BeFalse();
            textProvider.CompareTo(text.Length - 2, 3, "dog", true).Should().BeFalse();
        }

        [Test]
        [Category.Languages.Core]
        public void TextProvider_Boundary2() {
            var text = string.Empty;
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            textProvider.CompareTo(text.Length, 4, "laZy", true).Should().BeFalse();
            textProvider.CompareTo(0, 3, new string('c', 100), true).Should().BeFalse();
            textProvider.CompareTo(0, 0, "", true).Should().BeTrue();
            textProvider.CompareTo(0, 2, string.Empty, true).Should().BeFalse();
        }

        [Test]
        [Category.Languages.Core]
        public void TextProvider_Boundary3() {
            var text = "ab";
            var textBuffer = new TextBufferMock(text, "text");

            var textProvider = new TextProvider(textBuffer.CurrentSnapshot, 10);

            textProvider.CompareTo(text.Length, 4, "fooo", true).Should().BeFalse();
            textProvider.CompareTo(0, 3, new string('c', 100), true).Should().BeFalse();
            textProvider.CompareTo(0, 1, "a", true).Should().BeTrue();
            textProvider.CompareTo(0, 2, "ab", true).Should().BeTrue();
            textProvider.CompareTo(0, 2, "abc", true).Should().BeFalse();
            textProvider.CompareTo(1, 0, string.Empty, true).Should().BeTrue();
            textProvider.CompareTo(2, 0, string.Empty, true).Should().BeTrue();
        }
    }
}
