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
        public static bool IsWhitespaceBeforePosition(ITextProvider textProvider, int position) {
            char charBefore = position > 0 ? textProvider[position - 1] : 'x';
            return Char.IsWhiteSpace(charBefore);
        }

        /// <summary>
        /// Determines if there is only whitespace and a line break before position
        /// </summary>
        public static bool IsNewLineBeforePosition(ITextProvider textProvider, int position) {
            if (position > 0) {
                for (int i = position - 1; i >= 0; i--) {
                    char ch = textProvider[i];

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
        public static int LineBreaksBeforePosition(ITextProvider textProvider, int position) {
            int count = 0;

            if (position > 0) {
                for (int i = position - 1; i >= 0; i--) {
                    char ch = textProvider[i];

                    if (!Char.IsWhiteSpace(ch))
                        return count;

                    if (ch == '\r') {
                        count++;
                        if (i > 0 && textProvider[i - 1] == '\n') {
                            i--;
                        }
                    } else if (ch == '\n') {
                        count++;
                        if (i > 0 && textProvider[i - 1] == '\r') {
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
        public static int LineBreaksAfterPosition(ITextProvider textProvider, int position) {
            int count = 0;

            for (int i = position; i < textProvider.Length; i++) {
                char ch = textProvider[i];

                if (!Char.IsWhiteSpace(ch))
                    return count;

                if (ch == '\r') {
                    count++;
                    if (i < textProvider.Length - 1 && textProvider[i + 1] == '\n') {
                        i++;
                    }
                } else if (ch == '\n') {
                    count++;
                    if (i < textProvider.Length - 1 && textProvider[i + 1] == '\r') {
                        i++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Determines if there is only whitespace and a line break after position
        /// </summary>
        public static bool IsNewLineAfterPosition(ITextProvider textProvider, int position) {
            for (int i = position + 1; i < textProvider.Length; i++) {
                char ch = textProvider[i];

                if (!Char.IsWhiteSpace(ch))
                    return false;

                if (ch.IsLineBreak())
                    return true;
            }

            return false;
        }
    }
}
