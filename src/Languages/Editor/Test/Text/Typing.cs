// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Test.Text {
    [ExcludeFromCodeCoverage]
    public static class Typing {
        public static void Type(ITextBuffer textBuffer, string textToType) {
            Type(textBuffer, 0, textToType);
        }

        public static void Type(ITextBuffer textBuffer, int start, string textToType) {
            for (int i = 0; i < textToType.Length; i++) {
                textBuffer.Insert(start + i, textToType[i].ToString());
            }
        }

        public static void Backspace(ITextBuffer textBuffer, int start, int count) {
            RemoveChars(textBuffer, start, true, count);
        }

        public static void Delete(ITextBuffer textBuffer, int start, int count) {
            RemoveChars(textBuffer, start, false, count);
        }

        private static void RemoveChars(ITextBuffer textBuffer, int start, bool backspace, int count) {
            int offset = backspace ? -1 : 0;

            for (int i = 0; i < count; i++) {
                start += offset;
                textBuffer.Delete(new Span(start, 1));
            }
        }
    }
}
