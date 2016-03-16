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

                    if (!Char.IsWhiteSpace(ch))
                        return false;

                    if (ch.IsLineBreak()) {
                        newLinePosition = i;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
