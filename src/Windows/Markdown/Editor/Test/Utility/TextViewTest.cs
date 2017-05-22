// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class TextViewTest {
        public static ITextView MakeTextView(string content) {
            return MakeTextView(content, 0);
        }

        public static ITextView MakeTextView(string content, int caretPosition) {
            ITextBuffer textBuffer = new TextBufferMock(content, MdContentTypeDefinition.ContentType);
            return new TextViewMock(textBuffer, caretPosition);
        }

        public static ITextView MakeTextView(string content, ITextRange selectionRange) {
            TextBufferMock textBuffer = new TextBufferMock(content, MdContentTypeDefinition.ContentType);
            TextViewMock textView = new TextViewMock(textBuffer);

            textView.Selection.Select(new SnapshotSpan(textBuffer.CurrentSnapshot,
                     new Span(selectionRange.Start, selectionRange.Length)), false);
            return textView;
        }
    }
}
