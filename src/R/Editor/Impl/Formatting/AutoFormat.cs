#if NOT_YET
using System;
using System.Text;
using Microsoft.R.Editor.Selection;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.R.Editor.Formatting
{
    internal static class AutoFormat
    {
        public static void HandleAutoFormat(ITextView textView, ITextBuffer textBuffer)
        {
            if (!REditorSettings.AutoFormat)
                return;

            CaretPosition caretPosition = textView.Caret.Position;
            try
            {
                var line = textView.TextBuffer.CurrentSnapshot.GetLineFromPosition(caretPosition.BufferPosition);
                if (caretPosition.BufferPosition < line.End - 1)
                    return;
            }
            catch (ArgumentException)
            {
                return;
            }

            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            string bufferText = snapshot.GetText();

            var caretPosition = caretPoint.Value.Position;

            // Autoformatting takes time hence try not to format too much. since file may be huge
            // and poorly formatted so user types a semicolon and editor may spend goo few seconds
            // getting poorly formatted code into shape. *Correct* search for enclosing { } may also take
            // fair amount of time since we'd need to tokenize the entire file. Note that opening and 
            // closing braces may be off screen so using classification spans won't always help. 

            if ((caretPosition == bufferText.Length) ||
                (bufferText[caretPosition] != '}' && bufferText[caretPosition] != ';' && caretPosition > 0))
            {
                caretPosition--;
            }

            if (bufferText[caretPosition] == '}' || bufferText[caretPosition] == ';')
            {
                document.ScriptEntry.FormatSmart(FormattingOptionsTranslator.TranslateOptions(options), (uint)caretPosition, out isChanged);

                //sw.Stop();
                //Debug.WriteLine("Autoformat: {0}\r\n", sw.Elapsed);
            }

                var selectionTracker = new RSelectionTracker(textView, textBuffer);
                IncrementalTextChangeApplication.ApplyChange(textView, textBuffer, newText, Resources.Strings.AutoFormat, selectionTracker, 100);

                //sw.Stop();
                //Debug.WriteLine("Apply change: {0}\r\n", sw.Elapsed);
            }
    }
}
#endif
