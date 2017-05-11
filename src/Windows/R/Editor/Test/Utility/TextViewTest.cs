// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class TextViewTest {
        public static IEditorView MakeTextView(string content, out AstRoot ast) {
            return MakeTextView(content, 0, out ast);
        }

        public static IEditorView MakeTextViewRealTextBuffer(string content, IServiceContainer services) {
            var svc = services.GetService<ITextBufferFactoryService>();
            var rg = services.GetService<IContentTypeRegistryService>();
            var textBuffer = svc.CreateTextBuffer(content, rg.GetContentType(RContentTypeDefinition.ContentType));
            return new TextViewMock(textBuffer, 0).ToEditorView();
        }

        public static IEditorView MakeTextView(string content, int caretPosition, out AstRoot ast) {
            ast = RParser.Parse(content);
            var textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            return new TextViewMock(textBuffer, caretPosition).ToEditorView();
        }

        public static IEditorView MakeTextView(string content, ITextRange selectionRange) {
            var textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            var textView = new TextViewMock(textBuffer);

            textView.Selection.Select(new SnapshotSpan(textBuffer.CurrentSnapshot,
                     new Span(selectionRange.Start, selectionRange.Length)), false);
            return textView.ToEditorView();
        }
    }
}
