using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.EditorHelpers
{
    public static class TextViewHelpers
    {
        public static bool IsAutoInsertAllowed(ITextView textView)
        {
            return textView.Selection.Mode == TextSelectionMode.Stream;
        }

        public static bool IsAutoFormatAllowed(ITextView textView)
        {
            return textView.Selection.Mode == TextSelectionMode.Stream;
        }
    }
}
