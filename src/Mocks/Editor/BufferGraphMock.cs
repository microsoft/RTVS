// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class BufferGraphMock : IBufferGraph {
        private readonly IEnumerable<ITextBuffer> _textBuffers;

        public BufferGraphMock(IEnumerable<ITextBuffer> textBuffers) {
            _textBuffers = textBuffers;
            TopBuffer = textBuffers.First();
        }

        public BufferGraphMock(ITextBuffer textBuffer) {
            TopBuffer = textBuffer;
        }

        public ITextBuffer TopBuffer { get; }

#pragma warning disable 67
        public event EventHandler<GraphBufferContentTypeChangedEventArgs> GraphBufferContentTypeChanged;
        public event EventHandler<GraphBuffersChangedEventArgs> GraphBuffersChanged;

        public IMappingPoint CreateMappingPoint(SnapshotPoint point, PointTrackingMode trackingMode) => throw new NotImplementedException();
        public IMappingSpan CreateMappingSpan(SnapshotSpan span, SpanTrackingMode trackingMode) => throw new NotImplementedException();

        public Collection<ITextBuffer> GetTextBuffers(Predicate<ITextBuffer> match)
            => new Collection<ITextBuffer>(_textBuffers.Where(b => match(b)).ToList());

        public NormalizedSnapshotSpanCollection MapDownToBuffer(SnapshotSpan span, SpanTrackingMode trackingMode, ITextBuffer targetBuffer)
            => new NormalizedSnapshotSpanCollection(span);

        public SnapshotPoint? MapDownToBuffer(SnapshotPoint position, PointTrackingMode trackingMode,
            ITextBuffer targetBuffer, PositionAffinity affinity) => position;

        public NormalizedSnapshotSpanCollection MapDownToFirstMatch(SnapshotSpan span, SpanTrackingMode trackingMode, Predicate<ITextSnapshot> match)
            => new NormalizedSnapshotSpanCollection(span);

        public SnapshotPoint? MapDownToFirstMatch(SnapshotPoint position, PointTrackingMode trackingMode, Predicate<ITextSnapshot> match, PositionAffinity affinity)
            => position;

        public SnapshotPoint? MapDownToInsertionPoint(SnapshotPoint position, PointTrackingMode trackingMode, Predicate<ITextSnapshot> match)
            => position;

        public NormalizedSnapshotSpanCollection MapDownToSnapshot(SnapshotSpan span, SpanTrackingMode trackingMode, ITextSnapshot targetSnapshot)
            => new NormalizedSnapshotSpanCollection(span);

        public SnapshotPoint? MapDownToSnapshot(SnapshotPoint position, PointTrackingMode trackingMode, ITextSnapshot targetSnapshot, PositionAffinity affinity)
            => position;

        public NormalizedSnapshotSpanCollection MapUpToBuffer(SnapshotSpan span, SpanTrackingMode trackingMode, ITextBuffer targetBuffer)
            => new NormalizedSnapshotSpanCollection(span);

        public SnapshotPoint? MapUpToBuffer(SnapshotPoint point, PointTrackingMode trackingMode, PositionAffinity affinity, ITextBuffer targetBuffer)
            => point;

        public NormalizedSnapshotSpanCollection MapUpToFirstMatch(SnapshotSpan span, SpanTrackingMode trackingMode, Predicate<ITextSnapshot> match)
            => new NormalizedSnapshotSpanCollection(span);

        public SnapshotPoint? MapUpToFirstMatch(SnapshotPoint point, PointTrackingMode trackingMode, Predicate<ITextSnapshot> match, PositionAffinity affinity)
            => point;

        public NormalizedSnapshotSpanCollection MapUpToSnapshot(SnapshotSpan span, SpanTrackingMode trackingMode, ITextSnapshot targetSnapshot)
            => new NormalizedSnapshotSpanCollection(span);

        public SnapshotPoint? MapUpToSnapshot(SnapshotPoint point, PointTrackingMode trackingMode, PositionAffinity affinity, ITextSnapshot targetSnapshot)
            => point;
    }
}
