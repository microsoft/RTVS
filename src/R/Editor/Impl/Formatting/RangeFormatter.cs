using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Selection;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting
{
    internal static class RangeFormatter
    {
        public static void FormatSpan(ITextView textView, Span spanToFormat, RFormatOptions options)
        {
            ITextBuffer textBuffer = textView.TextBuffer;
            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;

            // Expand span to include the entire line
            ITextSnapshotLine startLine = snapshot.GetLineFromPosition(spanToFormat.Start);
            ITextSnapshotLine endLine = snapshot.GetLineFromPosition(spanToFormat.End);

            spanToFormat = Span.FromBounds(startLine.Start, endLine.End);
            string spanText = snapshot.GetText(spanToFormat.Start, spanToFormat.Length).Trim();

            RFormatter formatter = new RFormatter(options);
            string formattedText = formatter.Format(spanText);

            formattedText = formattedText.Trim(); // there may be inserted line breaks after {
            formattedText = AppendIndent(textView, spanToFormat.Start, formattedText, options);

            if (!spanText.Equals(formattedText, StringComparison.Ordinal))
            {
                var selectionTracker = new RSelectionTracker(textView, textBuffer);
                IncrementalTextChangeApplication.ApplyChange(textBuffer, spanToFormat.Start,
                    spanToFormat.Length, formattedText, Resources.Autoformat, selectionTracker, Int32.MaxValue);
            }
        }

        /// <summary>
        /// Appends indentation to each line so formatted text appears properly 
        /// indented inside the host document (script block in HTML page).
        /// </summary>
        private static string AppendIndent(ITextView textView, int position, string text, RFormatOptions options)
        {
            ITextBuffer textBuffer = textView.TextBuffer;
            ITextSnapshotLine line = textBuffer.CurrentSnapshot.GetLineFromPosition(position);
            string lineText = line.GetText();

            string leadingWhitespace = lineText.Substring(0, lineText.Length - lineText.TrimStart().Length);
            int textIndentInSpaces = IndentBuilder.TextIndentInSpaces(leadingWhitespace, options.TabSize);
            string indentString = IndentBuilder.GetIndentString(textIndentInSpaces, options.IndentType, options.TabSize);
            var sb = new StringBuilder();

            IList<string> lines = TextHelper.SplitTextIntoLines(text);

            for (int i = 0; i < lines.Count; i++)
            {
                lineText = lines[i];

                // Leave empty lines alone
                if (!string.IsNullOrWhiteSpace(lineText))
                    sb.Append(indentString);

                sb.Append(lineText);

                if (i < lines.Count - 1)
                    sb.Append("\r\n");
            }

            return sb.ToString();
        }
    }
}
