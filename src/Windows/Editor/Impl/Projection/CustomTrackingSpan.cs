// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System.Diagnostics;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Projection {
    /// <summary>
    /// This is a custom span which is like an EdgeInclusive span.  We need a custom span because elision buffers
    /// do not allow EdgeInclusive unless it spans the entire buffer.  We create snippets of our language spans
    /// and these are initially zero length.  When we insert at the beginning of these we'll end up keeping the
    /// span zero length if we're just EdgePositive tracking.
    /// </summary>
    [DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
    internal sealed class CustomTrackingSpan : ITrackingSpan {
        private readonly ITrackingPoint _start;
        private readonly ITrackingPoint _end;

        public CustomTrackingSpan(ITextSnapshot snapshot, Span span, PointTrackingMode startTracking, PointTrackingMode endTracking) {
            _start = snapshot.CreateTrackingPoint(span.Start, startTracking);
            _end = snapshot.CreateTrackingPoint(span.End, endTracking);
        }

        #region ITrackingSpan Members

        public SnapshotPoint GetEndPoint(ITextSnapshot snapshot) => _end.GetPoint(snapshot);

        public Span GetSpan(ITextVersion version) {
            return Span.FromBounds(_start.GetPosition(version), _end.GetPosition(version));
        }

        public SnapshotSpan GetSpan(ITextSnapshot snapshot) {
            return new SnapshotSpan(snapshot, Span.FromBounds(_start.GetPoint(snapshot), _end.GetPoint(snapshot)));
        }

        public SnapshotPoint GetStartPoint(ITextSnapshot snapshot) => _start.GetPoint(snapshot);
        public string GetText(ITextSnapshot snapshot) => GetSpan(snapshot).GetText();
        public ITextBuffer TextBuffer => _start.TextBuffer;
        public TrackingFidelityMode TrackingFidelity => TrackingFidelityMode.Forward;
        public SpanTrackingMode TrackingMode => SpanTrackingMode.Custom;
        #endregion

        private string GetDebuggerDisplay() {
            return "CustomSpan: " + GetSpan(_start.TextBuffer.CurrentSnapshot).ToString();
        }
    }
}
