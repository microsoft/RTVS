using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class TextViewTest {
        public static ITextView MakeTextView(string content, out AstRoot ast) {
            return MakeTextView(content, 0, out ast);
        }

        public static ITextView MakeTextViewRealTextBuffer(string content) {
            ITextBufferFactoryService svc = EditorShell.Current.ExportProvider.GetExportedValue<ITextBufferFactoryService>();
            IContentTypeRegistryService rg = EditorShell.Current.ExportProvider.GetExportedValue<IContentTypeRegistryService>();
            ITextBuffer textBuffer = svc.CreateTextBuffer(content, rg.GetContentType(RContentTypeDefinition.ContentType));
            return new TextViewMock(textBuffer, 0);
        }

        public static ITextView MakeTextView(string content, int caretPosition, out AstRoot ast) {
            ast = RParser.Parse(content);
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            return new TextViewMock(textBuffer, caretPosition);
        }

        public static ITextView MakeTextView(string content, ITextRange selectionRange) {
            TextBufferMock textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            TextViewMock textView = new TextViewMock(textBuffer);

            textView.Selection.Select(new SnapshotSpan(textBuffer.CurrentSnapshot,
                     new Span(selectionRange.Start, selectionRange.Length)), false);
            return textView;
        }
    }
}
