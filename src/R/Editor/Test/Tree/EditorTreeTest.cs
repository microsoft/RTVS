using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Test.Mocks;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Tree
{
    [ExcludeFromCodeCoverage]
    public static class EditorTreeTest
    {
        public static EditorTree ApplyTextChange(string expression, int start, int oldLength, int newLength, string newText)
        {
            TextBufferMock textBuffer = new TextBufferMock(expression, RContentTypeDefinition.ContentType);

            EditorTree tree = new EditorTree(textBuffer);
            tree.Build();

            TextChange tc = new TextChange();
            tc.OldRange = new TextRange(start, oldLength);
            tc.NewRange = new TextRange(start, newLength);
            tc.OldTextProvider = new TextProvider(textBuffer.CurrentSnapshot);

            if (oldLength == 0 && newText.Length > 0)
            {
                textBuffer.Insert(start, newText);
            }
            else if (oldLength > 0 && newText.Length > 0)
            {
                textBuffer.Replace(new Span(start, oldLength), newText);
            }
            else
            {
                textBuffer.Delete(new Span(start, oldLength));
            }

            return tree;
        }
    }
}
