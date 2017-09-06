// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Editor.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class TrackingPointMock : ITrackingPoint
    {
        int _position;

        public TrackingPointMock(ITextBuffer textBuffer, int position, PointTrackingMode trackingMode, TrackingFidelityMode trackingFidelity)
        {
            _position = position;
            
            TextBuffer = textBuffer;
            TrackingFidelity = trackingFidelity;
            TrackingMode = trackingMode;
        }

        #region ITrackingPoint Members

        public char GetCharacter(ITextSnapshot snapshot)
        {
            return snapshot.GetText(_position, 1)[0];
        }

        public SnapshotPoint GetPoint(ITextSnapshot snapshot)
        {
            return new SnapshotPoint(snapshot, _position);
        }

        public int GetPosition(ITextVersion version)
        {
            return _position;
        }

        public int GetPosition(ITextSnapshot snapshot)
        {
            return _position;
        }

        public ITextBuffer TextBuffer { get; private set;}
        public TrackingFidelityMode TrackingFidelity { get; private set;}
        public PointTrackingMode TrackingMode { get; private set;}

        #endregion
    }
}
