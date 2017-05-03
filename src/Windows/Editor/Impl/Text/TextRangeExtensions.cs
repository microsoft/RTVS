// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Editor.Text {
    public static class TextRangeExtensions {
        /// <summary>
        /// Converts text range to the editor span
        /// </summary>
        public static Span ToSpan(this ITextRange range) => new Span(range.Start, range.Length);

        /// <summary>
        /// Converts text range to the editor snapshot span
        /// </summary>
        public static SnapshotSpan ToSnapshotSpan(this ITextRange range, ITextSnapshot snapshot) => new SnapshotSpan(snapshot, range.Start, range.Length);

        /// <summary>
        /// Converts text range to the editor span
        /// </summary>
        public static ITextRange ToTextRange(this Span span) => new TextRange(span.Start, span.Length);

        /// <summary>
        /// Converts text range to the editor snapshot span
        /// </summary>
        public static ITextRange ToTextRange(this SnapshotSpan span) => new TextRange(span.Start.Position, span.Length);
        public static string GetText(this ITextRange range, ITextSnapshot snapshot) => snapshot.GetText(range.ToSpan());
        public static string GetText(this ITextRange range, ITextBuffer textBuffer) => range.GetText(textBuffer.CurrentSnapshot);
    }
}
