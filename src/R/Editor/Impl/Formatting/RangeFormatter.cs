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
            string spanText = snapshot.GetText(spanToFormat.Start, spanToFormat.Length);

            // Remove leading whitespace
            for (int i = 0; i < spanText.Length; i++)
            {
                char ch = spanText[i];

                if (!Char.IsWhiteSpace(ch) || ch == '\n' || ch == '\r')
                {
                    if (i > 0)
                        spanText = spanText.Substring(i);

                    break;
                }
            }

            // Remove any leading or training line breaks.
            // We will add it back after formatting if needed
            int startLineBreaks = 0;
            if (spanText.Length > 0 && (spanText[0] == '\n' || spanText[0] == '\r'))
            {
                spanText = spanText.Substring(1);
                startLineBreaks++;

                if (spanText.Length > 0 && (spanText[0] == '\n' || spanText[0] == '\r'))
                    spanText = spanText.Substring(1);
            }

            // Remove training whitespace
            var trailingWhitespace = new List<char>();
            for (int i = spanText.Length - 1; i >= 0; i--)
            {
                char ch = spanText[i];

                if (!Char.IsWhiteSpace(ch) || ch == '\n' || ch == '\r')
                    break;

                trailingWhitespace.Add(ch);
            }

            spanText = spanText.Substring(0, spanText.Length - trailingWhitespace.Count);

            int endLineBreaks = 0;
            if (spanText.Length > 0 && (spanText[spanText.Length - 1] == '\n' || spanText[spanText.Length - 1] == '\r'))
            {
                spanText = spanText.Substring(0, spanText.Length - 1);
                endLineBreaks++;

                if (spanText.Length > 0 && (spanText[spanText.Length - 1] == '\n' || spanText[spanText.Length - 1] == '\r'))
                    spanText = spanText.Substring(0, spanText.Length - 1);
            }


            spanText = TextHelper.RemoveIndent(spanText, options.IndentSize);
            RFormatter formatter = new RFormatter(REditorSettings.FormatOptions);
            string formattedText = formatter.Format(spanText);

            formattedText = AppendIndent(textView, spanToFormat.Start, formattedText);

            var sb = new StringBuilder();

            if (startLineBreaks > 0)
                sb.Append("\r\n");

            sb.Append(formattedText);

            if (endLineBreaks > 0)
                sb.Append("\r\n");

            if (trailingWhitespace.Count > 0)
            {
                trailingWhitespace.Reverse();
                var s = trailingWhitespace.ToArray();

                sb.Append(s);
            }

            formattedText = sb.ToString();

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
        private static string AppendIndent(ITextView textView, int position, string text)
        {
            ITextBuffer textBuffer = textView.TextBuffer;
            ITextSnapshotLine line = textBuffer.CurrentSnapshot.GetLineFromPosition(position);
            string lineText = line.GetText();

            RFormatOptions options = REditorSettings.FormatOptions;

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
