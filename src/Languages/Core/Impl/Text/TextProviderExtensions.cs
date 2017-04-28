// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;

namespace Microsoft.Languages.Core.Text {
    public static class TextProviderExtensions {
        public static bool IsWhitespaceBeforePosition(this ITextProvider textProvider, int position) {
            char charBefore = position > 0 ? textProvider[position - 1] : 'x';
            return Char.IsWhiteSpace(charBefore);
        }

        /// <summary>
        /// Detemines if there is nothing but whitespace between
        /// given position and preceding line break or beginning 
        /// of the file.
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="position">Position to check</param>
        public static bool IsNewLineBeforePosition(this ITextProvider textProvider, int position) {
            int newLinePosition; // Don't care about the value for this function overload
            return TryGetNewLineBeforePosition(textProvider, position, out newLinePosition);
        }

        public static bool TryGetNewLineBeforePosition(this ITextProvider textProvider, int position, out int newLinePosition) {
            newLinePosition = -1;
            if (position > 0) // fxcop fake-out
            {
                for (int i = position - 1; i >= 0; i--) {
                    char ch = textProvider[i];

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
            int count = 0;

            if (position > 0) // fxcop fake-out
            {
                for (int i = position - 1; i >= 0; i--) {
                    char ch = textProvider[i];

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
            int count = 0;

            for (int i = position; i < textProvider.Length; i++) {
                char ch = textProvider[i];

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
            for (int i = start; i < end; i++) {
                char ch = textProvider[i];
                if (!char.IsWhiteSpace(ch)) {
                    return false;
                }
                if (ch.IsLineBreak()) {
                    return false;
                }
            }
            return true;
        }
    }
}
