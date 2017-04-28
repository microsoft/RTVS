// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Common.Core;

namespace Microsoft.Languages.Core.Text {
    /// <summary>
    /// A helper class exposing various helper functions that 
    /// are used in formatting, smart indent and elsewhere else.
    /// </summary>
    public static class TextHelper {
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

                if (ch.IsLineBreak()) {
                    return true;
                }

                if (!Char.IsWhiteSpace(ch)) {
                    break;
                }
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
                if (ch.IsLineBreak()) {
                    lines.Add(text.Substring(lineStart, i - lineStart));

                    if (ch == '\r' && i + 1 < text.Length && text[i + 1] == '\n') {
                        i++;
                    } else if (ch == '\n' && i + 1 < text.Length && text[i + 1] == '\r') {
                        i++;
                    }

                    lineStart = i + 1;
                }
            }

            lines.Add(text.Substring(lineStart, text.Length - lineStart));
            return lines;
        }
    }
}
