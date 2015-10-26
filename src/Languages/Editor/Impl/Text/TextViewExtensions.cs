using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Text {
    public static class TextViewExtensions {
        /// <summary>
        /// Maps down to the buffer using positive point tracking and successor position affinity
        /// </summary>
        public static SnapshotPoint? MapDownToBuffer(this ITextView textView, int position, ITextBuffer buffer) {
            if (textView.BufferGraph == null) {
                // Unit test case
                return new SnapshotPoint(buffer.CurrentSnapshot, position);
            }
            return textView.BufferGraph.MapDownToBuffer(
                new SnapshotPoint(textView.TextBuffer.CurrentSnapshot, position),
                PointTrackingMode.Positive,
                buffer,
                PositionAffinity.Successor
            );
        }

        public static SnapshotPoint? MapUpToBuffer(this ITextView textView, int position, ITextBuffer buffer) {
            if (textView.BufferGraph == null) {
                // Unit test case
                return new SnapshotPoint(buffer.CurrentSnapshot, position);
            }
            return textView.BufferGraph.MapUpToBuffer(
                 new SnapshotPoint(
                     buffer.CurrentSnapshot,
                     position
                 ),
                 PointTrackingMode.Positive,
                 PositionAffinity.Successor,
                 textView.TextBuffer
             );
        }
    }
}
