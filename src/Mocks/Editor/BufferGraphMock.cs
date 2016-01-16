using System;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.VisualStudio.Editor.Mocks {
    public sealed class BufferGraphMock : IBufferGraph {
        public BufferGraphMock(ITextBuffer textBuffer) {
            TopBuffer = textBuffer;
        }

        public ITextBuffer TopBuffer { get; }

#pragma warning disable 67
        public event EventHandler<GraphBufferContentTypeChangedEventArgs> GraphBufferContentTypeChanged;
        public event EventHandler<GraphBuffersChangedEventArgs> GraphBuffersChanged;

        public IMappingPoint CreateMappingPoint(SnapshotPoint point, PointTrackingMode trackingMode) {
            throw new NotImplementedException();
        }

        public IMappingSpan CreateMappingSpan(SnapshotSpan span, SpanTrackingMode trackingMode) {
            throw new NotImplementedException();
        }

        public Collection<ITextBuffer> GetTextBuffers(Predicate<ITextBuffer> match) {
            throw new NotImplementedException();
        }

        public NormalizedSnapshotSpanCollection MapDownToBuffer(SnapshotSpan span, SpanTrackingMode trackingMode, ITextBuffer targetBuffer) {
            return new NormalizedSnapshotSpanCollection(span);
        }

        public SnapshotPoint? MapDownToBuffer(SnapshotPoint position, PointTrackingMode trackingMode, ITextBuffer targetBuffer, PositionAffinity affinity) {
            return position;
        }

        public NormalizedSnapshotSpanCollection MapDownToFirstMatch(SnapshotSpan span, SpanTrackingMode trackingMode, Predicate<ITextSnapshot> match) {
            return new NormalizedSnapshotSpanCollection(span);
        }

        public SnapshotPoint? MapDownToFirstMatch(SnapshotPoint position, PointTrackingMode trackingMode, Predicate<ITextSnapshot> match, PositionAffinity affinity) {
            return position;
        }

        public SnapshotPoint? MapDownToInsertionPoint(SnapshotPoint position, PointTrackingMode trackingMode, Predicate<ITextSnapshot> match) {
            return position;
        }

        public NormalizedSnapshotSpanCollection MapDownToSnapshot(SnapshotSpan span, SpanTrackingMode trackingMode, ITextSnapshot targetSnapshot) {
            return new NormalizedSnapshotSpanCollection(span);
        }

        public SnapshotPoint? MapDownToSnapshot(SnapshotPoint position, PointTrackingMode trackingMode, ITextSnapshot targetSnapshot, PositionAffinity affinity) {
            return position;
        }

        public NormalizedSnapshotSpanCollection MapUpToBuffer(SnapshotSpan span, SpanTrackingMode trackingMode, ITextBuffer targetBuffer) {
            return new NormalizedSnapshotSpanCollection(span);
        }

        public SnapshotPoint? MapUpToBuffer(SnapshotPoint point, PointTrackingMode trackingMode, PositionAffinity affinity, ITextBuffer targetBuffer) {
            return point;
        }

        public NormalizedSnapshotSpanCollection MapUpToFirstMatch(SnapshotSpan span, SpanTrackingMode trackingMode, Predicate<ITextSnapshot> match) {
            return new NormalizedSnapshotSpanCollection(span);
        }

        public SnapshotPoint? MapUpToFirstMatch(SnapshotPoint point, PointTrackingMode trackingMode, Predicate<ITextSnapshot> match, PositionAffinity affinity) {
            return point;
        }

        public NormalizedSnapshotSpanCollection MapUpToSnapshot(SnapshotSpan span, SpanTrackingMode trackingMode, ITextSnapshot targetSnapshot) {
            return new NormalizedSnapshotSpanCollection(span);
        }

        public SnapshotPoint? MapUpToSnapshot(SnapshotPoint point, PointTrackingMode trackingMode, PositionAffinity affinity, ITextSnapshot targetSnapshot) {
            return point;
        }
    }
}
