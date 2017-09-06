// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class TextVersionMock : ITextVersion {
        private TextChangeMock _change = new TextChangeMock();

        public TextVersionMock(ITextBuffer textBuffer, int version, int length) {
            TextBuffer = textBuffer;
            VersionNumber = version;
            ReiteratedVersionNumber = version;
            Length = length;
        }

        #region ITextVersion Members
        public INormalizedTextChangeCollection Changes => new TextChangeCollectionMock(_change);

        public ITrackingSpan CreateCustomTrackingSpan(Span span, TrackingFidelityMode trackingFidelity, object customState, CustomTrackToVersion behavior) 
            => new TrackingSpanMock(TextBuffer, span, SpanTrackingMode.EdgePositive, trackingFidelity);

        public ITrackingPoint CreateTrackingPoint(int position, PointTrackingMode trackingMode, TrackingFidelityMode trackingFidelity) 
            => TextBuffer.CurrentSnapshot.CreateTrackingPoint(position, trackingMode, trackingFidelity);

        public ITrackingPoint CreateTrackingPoint(int position, PointTrackingMode trackingMode) 
            => TextBuffer.CurrentSnapshot.CreateTrackingPoint(position, trackingMode);

        public ITrackingSpan CreateTrackingSpan(int start, int length, SpanTrackingMode trackingMode, TrackingFidelityMode trackingFidelity) 
            => TextBuffer.CurrentSnapshot.CreateTrackingSpan(start, length, trackingMode, trackingFidelity);

        public ITrackingSpan CreateTrackingSpan(int start, int length, SpanTrackingMode trackingMode)
            => TextBuffer.CurrentSnapshot.CreateTrackingSpan(start, length, trackingMode);

        public ITrackingSpan CreateTrackingSpan(Span span, SpanTrackingMode trackingMode, TrackingFidelityMode trackingFidelity)
            => TextBuffer.CurrentSnapshot.CreateTrackingSpan(span, trackingMode, trackingFidelity);

        public ITrackingSpan CreateTrackingSpan(Span span, SpanTrackingMode trackingMode)
            => TextBuffer.CurrentSnapshot.CreateTrackingSpan(span, trackingMode);

        public int Length { get; }
        public ITextVersion Next { get; private set; }

        public TextVersionMock CreateNextVersion(TextChangeMock change) {
            _change = change;
            var nextVersion = new TextVersionMock(TextBuffer, VersionNumber + 1, Length + _change.Delta);
            Next = nextVersion;
            return nextVersion;
        }

        public int ReiteratedVersionNumber { get; private set; }
        public ITextBuffer TextBuffer { get; private set; }
        public int VersionNumber { get; private set; }

        #endregion
    }
}
