// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor {
    public static class TextRangeExtensions {
        public static Span ToSpan(this ITextRange range) {
            return new Span(range.Start, range.Length);
        }

        public static string GetText(this ITextRange range, ITextSnapshot snapshot) {
            return snapshot.GetText(range.ToSpan());
        }

        public static string GetText(this ITextRange range, ITextBuffer textBuffer) {
            return range.GetText(textBuffer.CurrentSnapshot);
        }
    }
}
