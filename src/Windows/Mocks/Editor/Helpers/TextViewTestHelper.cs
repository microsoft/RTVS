// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Editor.Mocks.Helpers {
    [ExcludeFromCodeCoverage]
    public static class TextViewTestHelper {
        public static ITextView MakeTextViewRealTextBuffer(string content, string contentType, ITextBufferFactoryService svc, IContentTypeRegistryService rg) {
            ITextBuffer textBuffer = svc.CreateTextBuffer(content, rg.GetContentType(contentType));
            return new TextViewMock(textBuffer, 0);
        }

        public static ITextView MakeTextView(string content, string contentType, ITextRange selectionRange) {
            TextBufferMock textBuffer = new TextBufferMock(content, contentType);
            TextViewMock textView = new TextViewMock(textBuffer);

            textView.Selection.Select(new SnapshotSpan(textBuffer.CurrentSnapshot,
                     new Span(selectionRange.Start, selectionRange.Length)), false);
            return textView;
        }
    }
}
