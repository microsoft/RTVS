using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Test.Mocks
{
    [ExcludeFromCodeCoverage]
    public class TextVersionMock : ITextVersion
    {
        TextChangeMock _change;

        public TextVersionMock(ITextBuffer textBuffer, int version, int length)
        {
            TextBuffer = textBuffer;
            VersionNumber = version;
            ReiteratedVersionNumber = version;
            Length = length;

            _change = new TextChangeMock();
         }

        #region ITextVersion Members
        public INormalizedTextChangeCollection Changes
        {
            get { return new TextChangeCollectionMock(_change); }
        }

        public ITrackingSpan CreateCustomTrackingSpan(Span span, TrackingFidelityMode trackingFidelity, object customState, CustomTrackToVersion behavior)
        {
            return new TrackingSpanMock(TextBuffer, span, SpanTrackingMode.EdgePositive, trackingFidelity);
        }

        public ITrackingPoint CreateTrackingPoint(int position, PointTrackingMode trackingMode, TrackingFidelityMode trackingFidelity)
        {
            return TextBuffer.CurrentSnapshot.CreateTrackingPoint(position, trackingMode, trackingFidelity);
        }

        public ITrackingPoint CreateTrackingPoint(int position, PointTrackingMode trackingMode)
        {
            return TextBuffer.CurrentSnapshot.CreateTrackingPoint(position, trackingMode);
        }

        public ITrackingSpan CreateTrackingSpan(int start, int length, SpanTrackingMode trackingMode, TrackingFidelityMode trackingFidelity)
        {
            return TextBuffer.CurrentSnapshot.CreateTrackingSpan(start, length, trackingMode, trackingFidelity);
        }

        public ITrackingSpan CreateTrackingSpan(int start, int length, SpanTrackingMode trackingMode)
        {
            return TextBuffer.CurrentSnapshot.CreateTrackingSpan(start, length, trackingMode);
        }

        public ITrackingSpan CreateTrackingSpan(Span span, SpanTrackingMode trackingMode, TrackingFidelityMode trackingFidelity)
        {
            return TextBuffer.CurrentSnapshot.CreateTrackingSpan(span, trackingMode, trackingFidelity);
        }

        public ITrackingSpan CreateTrackingSpan(Span span, SpanTrackingMode trackingMode)
        {
            return TextBuffer.CurrentSnapshot.CreateTrackingSpan(span, trackingMode);
        }

        public int Length
        {
            get; private set;
        }

        public ITextVersion Next { get; private set; }

        public TextVersionMock CreateNextVersion(TextChangeMock change)
        {
            _change = change;
            TextVersionMock nextVersion = new TextVersionMock(TextBuffer, VersionNumber + 1, Length + _change.Delta);

            Next = nextVersion;

            return nextVersion;
        }

        public int ReiteratedVersionNumber { get; private set; }
        public ITextBuffer TextBuffer { get; private set; }
        public int VersionNumber { get; private set; }

        #endregion
    }
}
