// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Formatting;

namespace Microsoft.VisualStudio.Editor.Mocks {
    public sealed class TextViewLineMock : ITextViewLine {
        private ITextSnapshotLine _line;

        public TextViewLineMock(ITextSnapshotLine line) {
            _line = line;
        }

        public double Baseline => 0;
        public double Bottom => 0;

        public TextViewLineChange Change {
            get { throw new NotImplementedException(); }
        }
        public LineTransform DefaultLineTransform {
            get { throw new NotImplementedException(); }
        }

        public double DeltaY => 0;
        public SnapshotPoint End => _line.End;
        public SnapshotPoint EndIncludingLineBreak => _line.EndIncludingLineBreak;
        public double EndOfLineWidth => 0;
        public SnapshotSpan Extent => _line.Extent;

        public IMappingSpan ExtentAsMappingSpan {
            get { throw new NotImplementedException(); }
        }
        public SnapshotSpan ExtentIncludingLineBreak => _line.ExtentIncludingLineBreak;

        public IMappingSpan ExtentIncludingLineBreakAsMappingSpan {
            get { throw new NotImplementedException(); }
        }

        public double Height => 0;
        public object IdentityTag => null;
        public bool IsFirstTextViewLineForSnapshotLine => true;
        public bool IsLastTextViewLineForSnapshotLine => true;
        public bool IsValid => true;
        public double Left => 0;
        public int Length => _line.Length;
        public int LengthIncludingLineBreak => _line.EndIncludingLineBreak;
        public int LineBreakLength => _line.LineBreakLength;

        public LineTransform LineTransform {
            get { throw new NotImplementedException(); }
        }

        public double Right => 0;
        public ITextSnapshot Snapshot => _line.Snapshot;
        public SnapshotPoint Start => _line.Start;
        public double TextBottom => 0;
        public double TextHeight => 0;
        public double TextLeft => 0;
        public double TextRight => 0;
        public double TextTop => 0;
        public double TextWidth => 0;
        public double Top => 0;
        public double VirtualSpaceWidth => 0;
        public VisibilityState VisibilityState => VisibilityState.FullyVisible;
        public double Width => 0;

        public bool ContainsBufferPosition(SnapshotPoint bufferPosition) {
            return _line.Start <= bufferPosition.Position && _line.End > bufferPosition.Position;
        }

        public TextBounds? GetAdornmentBounds(object identityTag) => null;
        public ReadOnlyCollection<object> GetAdornmentTags(object providerTag) => null;

        public SnapshotPoint? GetBufferPositionFromXCoordinate(double xCoordinate) {
            throw new NotImplementedException();
        }

        public SnapshotPoint? GetBufferPositionFromXCoordinate(double xCoordinate, bool textOnly) {
            throw new NotImplementedException();
        }

        public TextBounds GetCharacterBounds(VirtualSnapshotPoint bufferPosition) {
            throw new NotImplementedException();
        }

        public TextBounds GetCharacterBounds(SnapshotPoint bufferPosition) {
            throw new NotImplementedException();
        }

        public TextBounds GetExtendedCharacterBounds(VirtualSnapshotPoint bufferPosition) {
            throw new NotImplementedException();
        }

        public TextBounds GetExtendedCharacterBounds(SnapshotPoint bufferPosition) {
            throw new NotImplementedException();
        }

        public VirtualSnapshotPoint GetInsertionBufferPositionFromXCoordinate(double xCoordinate) {
            throw new NotImplementedException();
        }

        public Collection<TextBounds> GetNormalizedTextBounds(SnapshotSpan bufferSpan) {
            throw new NotImplementedException();
        }

        public SnapshotSpan GetTextElementSpan(SnapshotPoint bufferPosition) {
            throw new NotImplementedException();
        }

        public VirtualSnapshotPoint GetVirtualBufferPositionFromXCoordinate(double xCoordinate) {
            throw new NotImplementedException();
        }

        public bool IntersectsBufferSpan(SnapshotSpan bufferSpan) {
            throw new NotImplementedException();
        }
    }
}
