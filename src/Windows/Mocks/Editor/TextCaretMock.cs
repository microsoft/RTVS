// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class TextCaretMock : ITextCaret {
        private int _position;
        private ITextView _textView;

        public TextCaretMock(ITextView textView, int position) {
            _textView = textView;
            _position = position;
        }

        public double Bottom => 100;

        public ITextViewLine ContainingTextViewLine {
            get { return new TextViewLineMock(_textView.TextBuffer.CurrentSnapshot.GetLineFromPosition(_position)); }
        }

        public double Height => 100;
        public bool InVirtualSpace => false;

        public bool IsHidden { get; set; }

        public double Left => 0;
        public bool OverwriteMode => false;

        public CaretPosition Position {
            get {
                return new CaretPosition(
                    new VirtualSnapshotPoint(_textView.TextBuffer.CurrentSnapshot, _position),
                    new MappingPointMock(_textView.TextBuffer, _position),
                    PositionAffinity.Successor);
            }
        }

        public double Right => 1;
        public double Top => 0;
        public double Width => 1;

        public void EnsureVisible() {
        }

        public CaretPosition MoveTo(VirtualSnapshotPoint bufferPosition) {
            return MoveTo(bufferPosition, PositionAffinity.Successor);
        }

        public CaretPosition MoveTo(SnapshotPoint bufferPosition) {
            return MoveTo(bufferPosition, PositionAffinity.Successor);
        }

        public CaretPosition MoveTo(ITextViewLine textLine) {
            return MoveTo(new SnapshotPoint(textLine.Snapshot, textLine.Start));
        }

        public CaretPosition MoveTo(SnapshotPoint bufferPosition, PositionAffinity caretAffinity) {
            _position = bufferPosition.Position;

            return new CaretPosition(new VirtualSnapshotPoint(bufferPosition),
                  new MappingPointMock(bufferPosition.Snapshot.TextBuffer, bufferPosition.Position),
                  caretAffinity);
        }

        public CaretPosition MoveTo(VirtualSnapshotPoint bufferPosition, PositionAffinity caretAffinity) {
            _position = bufferPosition.Position.Position;

            return new CaretPosition(bufferPosition,
                  new MappingPointMock(bufferPosition.Position.Snapshot.TextBuffer, bufferPosition.Position),
                  PositionAffinity.Successor);
        }

        public CaretPosition MoveTo(ITextViewLine textLine, double xCoordinate) {
            throw new NotImplementedException();
        }

        public CaretPosition MoveTo(VirtualSnapshotPoint bufferPosition, PositionAffinity caretAffinity, bool captureHorizontalPosition) {
            return MoveTo(bufferPosition, caretAffinity);
        }

        public CaretPosition MoveTo(SnapshotPoint bufferPosition, PositionAffinity caretAffinity, bool captureHorizontalPosition) {
            return MoveTo(bufferPosition, caretAffinity);
        }

        public CaretPosition MoveTo(ITextViewLine textLine, double xCoordinate, bool captureHorizontalPosition) {
            throw new NotImplementedException();
        }

        public CaretPosition MoveToNextCaretPosition() {
            throw new NotImplementedException();
        }

        public CaretPosition MoveToPreferredCoordinates() {
            throw new NotImplementedException();
        }

        public CaretPosition MoveToPreviousCaretPosition() {
            throw new NotImplementedException();
        }

#pragma warning disable 0067
        public event EventHandler<CaretPositionChangedEventArgs> PositionChanged;
    }
}
