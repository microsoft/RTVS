// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Formatting {
    /// <summary>
    /// Various whitespace operations
    /// </summary>
    public static class Whitespace {
        /// <summary>
        /// Determines if there is a whitespace before given position
        /// </summary>
        public static bool IsWhitespaceBeforePosition(ITextIterator iterator, int position) {
            char charBefore = position > 0 ? iterator[position - 1] : 'x';
            return Char.IsWhiteSpace(charBefore);
        }

        /// <summary>
        /// Determines if there is only whitespace and a line break before position
        /// </summary>
        public static bool IsNewLineBeforePosition(ITextIterator iterator, int position) {
            if (position > 0) {
                for (int i = position - 1; i >= 0; i--) {
                    char ch = iterator[i];

                    if (!Char.IsWhiteSpace(ch))
                        return false;

                    if (ch.IsLineBreak())
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines number of line breaks before position
        /// </summary>
        public static int LineBreaksBeforePosition(ITextIterator iterator, int position) {
            int count = 0;

            if (position > 0) {
                for (int i = position - 1; i >= 0; i--) {
                    char ch = iterator[i];

                    if (!Char.IsWhiteSpace(ch))
                        return count;

                    if (ch == '\r') {
                        count++;
                        if (i > 0 && iterator[i - 1] == '\n') {
                            i--;
                        }
                    } else if (ch == '\n') {
                        count++;
                        if (i > 0 && iterator[i - 1] == '\r') {
                            i--;
                        }
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Determines number of line breaks after position
        /// </summary>
        public static int LineBreaksAfterPosition(ITextIterator iterator, int position) {
            int count = 0;

            for (int i = position; i < iterator.Length; i++) {
                char ch = iterator[i];

                if (!Char.IsWhiteSpace(ch))
                    return count;

                if (ch == '\r') {
                    count++;
                    if (i < iterator.Length - 1 && iterator[i + 1] == '\n') {
                        i++;
                    }
                } else if (ch == '\n') {
                    count++;
                    if (i < iterator.Length - 1 && iterator[i + 1] == '\r') {
                        i++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Determines if there is only whitespace and a line break after position
        /// </summary>
        public static bool IsNewLineAfterPosition(ITextProvider iterator, int position) {
            for (int i = position + 1; i < iterator.Length; i++) {
                char ch = iterator[i];

                if (!Char.IsWhiteSpace(ch))
                    return false;

                if (ch.IsLineBreak())
                    return true;
            }

            return false;
        }
    }
}
