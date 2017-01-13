// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Components.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Xunit;

namespace Microsoft.R.Editor.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Category.R.Completion]
    public class TextViewExtensionsTest {
        [CompositeTest]
        [InlineData("", 0,"")]
        [InlineData("x", 0, "x")]
        [InlineData("x(", 1, "x")]
        [InlineData("`a`", 1, "`a`")]
        [InlineData("`a`", 2, "`a`")]
        [InlineData("abc$def", 1, "abc")]
        [InlineData("abc$def", 5, "def")]
        [InlineData("`a bc`@`d ef `", 1, "`a bc`")]
        [InlineData("`a bc`@`d ef `", 8, "`d ef `")]
        [InlineData("#item", 2, "item")]
        [InlineData("x <- 1 # item", 10, "item")]
        [InlineData("#", 1, "")]
        [InlineData("> #", 3, "")]
        [InlineData("> #", 2, "")]
        public void GetIdentifierUnderCaret(string content, int caretPosition, string expected) {
            var textView = MakeTextView(content, caretPosition);
            Span span;
            textView.GetIdentifierUnderCaret(out span);
            string actual = textView.TextBuffer.CurrentSnapshot.GetText(span);

            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("", 0, "")]
        [InlineData("x", 1, "x")]
        [InlineData("x(", 1, "x")]
        [InlineData("x(", 2, "")]
        [InlineData("`a`", 3, "`a`")]
        [InlineData("abc$def@", 8, "abc$def@")]
        [InlineData("abc$def@", 4, "abc$")]
        [InlineData("abc", 3, "abc")]
        [InlineData("`a bc`@`d ef `$", 7, "`a bc`@")]
        [InlineData("`a bc`@`d ef `", 14, "`a bc`@`d ef `")]
        [InlineData("`a bc` `d ef `$", 15, "`d ef `$")]
        [InlineData("x$+y$", 5, "y$")]
        [InlineData("x$+`y y`$", 9, "`y y`$")]
        [InlineData("a::b$`z`", 8, "a::b$`z`")]
        [InlineData("#", 1, "")]
        [InlineData("> #", 3, "")]
        [InlineData("> #", 2, "")]
        public void GetVariableBeforeCaret(string content, int caretPosition, string expected) {
            var textView = MakeTextView(content, caretPosition);
            string actual = textView.GetVariableNameBeforeCaret();
            actual.Should().Be(expected);
        }

        private ITextView MakeTextView(string content, int caretPosition) {
            var tb = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            var tv = new TextViewMock(tb);
            tv.Caret.MoveTo(new SnapshotPoint(tb.CurrentSnapshot, caretPosition));
            return tv;
        }
    }
}
