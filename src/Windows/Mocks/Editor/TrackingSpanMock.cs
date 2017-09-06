// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Editor.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class TrackingSpanMock : ITrackingSpan
    {
        Span _span;

        public TrackingSpanMock(ITextBuffer textBuffer, Span span, SpanTrackingMode trackingMode, TrackingFidelityMode trackingFidelity)
        {
            _span = span;

            TextBuffer = textBuffer;
            TrackingMode = trackingMode;
            TrackingFidelity = trackingFidelity;

            var mock = textBuffer as TextBufferMock;
            mock.BeforeChanged += OnBeforeTextBufferChanged;
        }

        void OnBeforeTextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            foreach (var change in e.Changes)
            {
                var offset = change.NewLength - change.OldLength;

                if (change.OldEnd < _span.Start)
                {
                    _span = new Span(_span.Start + offset, _span.Length);
                }
                else if (change.OldPosition > _span.End)
                {
                }
                else if (change.OldPosition == _span.End && _span.Length == 0)
                {
                    _span = new Span(_span.Start, _span.Length + offset);
                }
                else if (_span.Contains(change.OldPosition) &&
                        (_span.Contains(change.OldEnd) || _span.End == change.OldEnd) &&
                        (_span.Contains(change.NewEnd) || _span.End == change.NewEnd))
                {
                    _span = new Span(_span.Start, _span.Length + offset);
                }
            }
        }

        #region ITrackingSpan Members

        public SnapshotPoint GetEndPoint(ITextSnapshot snapshot)
        {
            return new SnapshotPoint(snapshot, _span.End);
        }

        public Span GetSpan(ITextVersion version)
        {
            return _span;
        }

        public SnapshotSpan GetSpan(ITextSnapshot snapshot)
        {
            return new SnapshotSpan(snapshot, _span);
        }

        public SnapshotPoint GetStartPoint(ITextSnapshot snapshot)
        {
            return new SnapshotPoint(snapshot, _span.Start);
        }

        public string GetText(ITextSnapshot snapshot)
        {
            return snapshot.GetText(_span);
        }

        public ITextBuffer TextBuffer { get; private set;}
        public TrackingFidelityMode TrackingFidelity { get; private set;}
        public SpanTrackingMode TrackingMode { get; private set;}

        #endregion
    }
}
