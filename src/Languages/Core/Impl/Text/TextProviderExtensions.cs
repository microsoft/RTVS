// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;

namespace Microsoft.Languages.Core.Text {
    public static class TextProviderExtensions {
        public static bool IsWhitespaceBeforePosition(this ITextProvider textProvider, int position) {
            var charBefore = position > 0 ? textProvider[position - 1] : 'x';
            return char.IsWhiteSpace(charBefore);
        }

        public static bool IsWhitespaceAfterPosition(this ITextProvider textProvider, int position) {
            var charAfter = position < textProvider.Length ? textProvider[position + 1] : 'x';
            return char.IsWhiteSpace(charAfter);
        }

        /// <summary>
        /// Detemines if there is nothing but whitespace between
        /// given position and preceding line break or beginning 
        /// of the file.
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="position">Position to check</param>
        public static bool IsNewLineBeforePosition(this ITextProvider textProvider, int position)
            =>  TryGetNewLineBeforePosition(textProvider, position, out int newLinePosition);

        public static bool TryGetNewLineBeforePosition(this ITextProvider textProvider, int position, out int newLinePosition) {
            newLinePosition = -1;
            if (position > 0) // fxcop fake-out
            {
                for (var i = position - 1; i >= 0; i--) {
                    var ch = textProvider[i];

                    if (!Char.IsWhiteSpace(ch)) {
                        return false;
                    }

                    if (ch.IsLineBreak()) {
                        newLinePosition = i;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Counts number of line breaks between position and the nearest 
        /// non-whitespace character that precedes the position
        /// </summary>
        public static int LineBreaksBeforePosition(this ITextProvider textProvider, int position) {
            var count = 0;

            if (position > 0) // fxcop fake-out
            {
                for (var i = position - 1; i >= 0; i--) {
                    var ch = textProvider[i];

                    if (!Char.IsWhiteSpace(ch)) {
                        return count;
                    }

                    if (ch == '\r' || ch == '\n') {
                        if (i > 0 && (textProvider[i - 1] == '\r' || textProvider[i - 1] == '\n')) {
                            i--;
                        }

                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Counts number of line breaks between position and the nearest 
        /// non-whitespace character that follows the position
        /// </summary>
        public static int LineBreaksAfterPosition(this ITextProvider textProvider, int position) {
            var count = 0;

            for (var i = position; i < textProvider.Length; i++) {
                var ch = textProvider[i];

                if (!Char.IsWhiteSpace(ch)) {
                    return count;
                }

                if (ch == '\r' || ch == '\n') {
                    if (i < textProvider.Length - 1 && (textProvider[i + 1] == '\r' || textProvider[i + 1] == '\n')) {
                        i++;
                    }

                    count++;
                }
            }

            return count;
        }

        public static bool IsWhiteSpaceOnlyRange(this ITextProvider textProvider, int start, int end) {
            if (end < start) {
                end = textProvider.Length;
            }
            for (var i = start; i < end; i++) {
                var ch = textProvider[i];
                if (!char.IsWhiteSpace(ch)) {
                    return false;
                }
                if (ch.IsLineBreak()) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Determines if there is nothing but whitespace between
        /// given positions (end is non-inclusive).
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="start">Start position (inclusive)</param>
        /// <param name="end">End position (non-inclusive)</param>
        public static bool IsWhitespaceOnlyBetweenPositions(this ITextProvider textProvider, int start, int end) {
            end = Math.Min(textProvider.Length, end);
            for (var i = start; i < end; i++) {
                var ch = textProvider[i];

                if (!char.IsWhiteSpace(ch)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if there is nothing but whitespace between
        /// given position and the next line break or end of file.
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="position">Position to check</param>
        public static bool IsNewLineAfterPosition(this ITextProvider textProvider, int position) {

            // Walk backwards from the artifact position
            for (var i = position; i < textProvider.Length; i++) {
                var ch = textProvider[i];

                if (ch.IsLineBreak()) {
                    return true;
                }

                if (!char.IsWhiteSpace(ch)) {
                    break;
                }
            }

            return false;
        }
    }
}
