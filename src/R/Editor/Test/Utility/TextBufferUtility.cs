using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Utility
{
    public static class TextBufferUtility
    {
        public static void ApplyTextChange(ITextBuffer textBuffer, int start, int oldLength, int newLength, string newText)
        {
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
        }
    }
}
