using Microsoft.VisualStudio.Text;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Editor.Text {
    public static class TextRangeExtensions {
        /// <summary>
        /// Converts text range to the editor span
        /// </summary>
        public static Span ToSpan(this ITextRange range) {
            return new Span(range.Start, range.Length);
        }

        /// <summary>
        /// Converts text range to the editor snapshot span
        /// </summary>
        public static SnapshotSpan ToSnapshotSpan(this ITextRange range, ITextSnapshot snapshot) {
            return new SnapshotSpan(snapshot, range.Start, range.Length);
        }

        /// <summary>
        /// Converts text range to the editor span
        /// </summary>
        public static ITextRange ToTextRange(this Span span) {
            return new TextRange(span.Start, span.Length);
        }

        /// <summary>
        /// Converts text range to the editor snapshot span
        /// </summary>
        public static ITextRange ToTextRange(this SnapshotSpan span) {
            return new TextRange(span.Start.Position, span.Length);
        }
    }
}
