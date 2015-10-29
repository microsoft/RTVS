using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Languages.Core.Text {
    /// <summary>
    /// A helper class exposing various helper functions that 
    /// are used in formatting, smart indent and elsewhere else.
    /// </summary>
    public static class TextHelper {
        public static char[] EndOfLineChars = { '\r', '\n' };

        /// <summary>
        /// Determines if there is nothing but whitespace between
        /// given position and the next line break or end of file.
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="position">Position to check</param>
        public static bool IsNewLineAfterPosition(ITextProvider textProvider, int position) {

            // Walk backwards from the artifact position
            for (int i = position; i < textProvider.Length; i++) {
                char ch = textProvider[i];

                if (ch == '\n' || ch == '\r')
                    return true;

                if (!Char.IsWhiteSpace(ch))
                    break;
            }

            return false;
        }

        /// <summary>
        /// Determines if there is nothing but whitespace between
        /// given positions (end is non-inclusive).
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="position">Start position (inclusive)</param>
        /// <param name="position">End position (non-inclusive)</param>
        public static bool IsWhitespaceOnlyBetweenPositions(ITextProvider textProvider, int start, int end) {
            end = Math.Min(textProvider.Length, end);
            for (int i = start; i < end; i++) {
                char ch = textProvider[i];

                if (!Char.IsWhiteSpace(ch)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Splits string into lines based on line breaks
        /// </summary>
        public static IList<string> SplitTextIntoLines(string text) {
            var lines = new List<string>();
            int lineStart = 0;

            for (int i = 0; i < text.Length; i++) {
                char ch = text[i];
                if (ch == '\r' || ch == '\n') {
                    lines.Add(text.Substring(lineStart, i - lineStart));

                    // Skip '\n' but only in "\r\n" sequence. 
                    if (ch == '\r' && i + 1 < text.Length && text[i + 1] == '\n') {
                        i++;
                    }

                    lineStart = i + 1;
                }
            }

            lines.Add(text.Substring(lineStart, text.Length - lineStart));

            return lines;
        }

        public static string ConvertTabsToSpaces(string text, int tabSize) {
            var sb = new StringBuilder(text.Length);
            int charsSoFar = 0;

            for (int i = 0; i < text.Length; i++) {
                char ch = text[i];

                if (ch == '\t') {
                    var spaces = tabSize - (charsSoFar % tabSize);
                    sb.Append(' ', spaces);
                    charsSoFar = 0;
                } else if (ch == '\r' || ch == '\n') {
                    charsSoFar = 0;
                    sb.Append(ch);
                } else {
                    charsSoFar++;
                    charsSoFar = charsSoFar % tabSize;
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }

        public static int MeasureLeadingWhitespace(string text, int tabSize) {
            int spacesSoFar = 0;
            int size = 0;
            for (int i = 0; i < text.Length; i++) {
                char ch = text[i];

                if (ch == '\t') {
                    var spaces = tabSize - (spacesSoFar % tabSize);
                    size += spaces;
                    spacesSoFar = 0;
                } else if (!Char.IsWhiteSpace(ch) || ch == '\r' || ch == '\n') {
                    break;
                } else {
                    spacesSoFar++;
                    spacesSoFar = spacesSoFar % tabSize;
                    size++;
                }
            }

            return size;
        }

        public static string RemoveIndent(string text, int tabSize) {
            // Normalize to spaces
            text = ConvertTabsToSpaces(text, tabSize);

            var lines = TextHelper.SplitTextIntoLines(text);

            // Measure how much whitespace is before each line and find minimal whitespace 
            // properly counting tabs and spaces. We convert tab to spaces when counting.
            // Store leading whitespace length for each line.

            var leadingWSLengthInChars = new int[lines.Count];

            for (int i = 0; i < lines.Count; i++) {
                var line = lines[i];

                leadingWSLengthInChars[i] = 0;

                if (line.Length > 0 && !String.IsNullOrWhiteSpace(line)) {
                    for (int j = 0; j < line.Length; j++) {
                        char ch = line[j];

                        if (!Char.IsWhiteSpace(ch))
                            break;

                        leadingWSLengthInChars[i]++;
                    }
                } else {
                    leadingWSLengthInChars[i] = Int32.MaxValue;
                }
            }

            int minWsInChars = Int32.MaxValue;
            for (int i = 0; i < lines.Count; i++) {
                minWsInChars = Math.Min(minWsInChars, leadingWSLengthInChars[i]);
            }

            // Now we know line wth smallest leading whitespace. We need to trim other lines 
            // leading whitespace by this amount and convert remaining leading whitespace
            // to tabs or spaces according to the formatting options.
            // Generate indenting whitespace for each line according to base block indent 
            // and current formatting options.

            var sb = new StringBuilder();

            for (int i = 0; i < lines.Count; i++) {
                var line = lines[i];

                if (!String.IsNullOrEmpty(line) && leadingWSLengthInChars[i] != Int32.MaxValue) {
                    sb.Append(lines[i].Substring(minWsInChars));
                }

                if (i < lines.Count - 1) {
                    sb.Append("\r\n");
                }
            }

            return sb.ToString();
        }

        public static bool LeadingWhitespaceContainsLineBreak(string text) {
            for (int i = 0; i < text.Length; i++) {
                char ch = text[i];

                if (ch == '\r' || ch == '\n')
                    return true;

                if (!Char.IsWhiteSpace(ch))
                    break;
            }

            return false;
        }

        public static bool TrailingWhitespaceContainsLineBreak(string text) {
            for (int i = text.Length - 1; i >= 0; i--) {
                char ch = text[i];

                if (ch == '\r' || ch == '\n')
                    return true;

                if (!Char.IsWhiteSpace(ch))
                    break;
            }

            return false;
        }

        public static string GetCurrentLineText(this ITextProvider textProvider, int position) {
            int start = 0;
            int end = 0;

            for (int i = position; i >= 0; i--) {
                char ch = textProvider[i];

                if (ch == '\r' || ch == '\n') {
                    start = i + 1;
                    break;
                }
            }

            for (int i = position; i < textProvider.Length; i++) {
                char ch = textProvider[i];

                if (ch == '\r' || ch == '\n') {
                    end = i;
                    break;
                }
            }

            if (start > end) {
                start = end;
            }

            return textProvider.GetText(TextRange.FromBounds(start, end));
        }
    }
}
