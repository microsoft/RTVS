using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Test.Mocks;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Test.Utility
{
    public static class TextViewTest
    {
        public static ITextView MakeTextView(string content)
        {
            return MakeTextView(content, 0);
        }

        public static ITextView MakeTextView(string content, int caretPosition)
        {
            EditorShell.SetShell(TestEditorShell.Create());

            ITextBuffer textBuffer = new TextBufferMock(content, MdContentTypeDefinition.ContentType);
            return new TextViewMock(textBuffer, caretPosition);
        }

        public static ITextView MakeTextView(string content, ITextRange selectionRange)
        {
            TextBufferMock textBuffer = new TextBufferMock(content, MdContentTypeDefinition.ContentType);
            TextViewMock textView = new TextViewMock(textBuffer);

            textView.Selection.Select(new SnapshotSpan(textBuffer.CurrentSnapshot, 
                     new Span(selectionRange.Start, selectionRange.Length)), false);
            return textView;
        }
    }
}
