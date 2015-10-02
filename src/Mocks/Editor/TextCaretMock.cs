using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace Microsoft.VisualStudio.Editor.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class TextCaretMock : ITextCaret
    {
        private int _position;
        private ITextView _textView;

        public TextCaretMock(ITextView textView, int position)
        {
            _textView = textView;
            _position = position;
        }

        public double Bottom
        {
            get { return 100; }
        }

        public ITextViewLine ContainingTextViewLine
        {
            get { return null; }
        }

        public double Height
        {
            get { return 100; }
        }

        public bool InVirtualSpace
        {
            get { return false; }
        }

        public bool IsHidden { get; set; }

        public double Left
        {
            get { return 0; }
        }

        public bool OverwriteMode
        {
            get { return false; }
        }

        public CaretPosition Position
        {
            get
            {
                return new CaretPosition(
                    new VirtualSnapshotPoint(_textView.TextBuffer.CurrentSnapshot, _position),
                    new MappingPointMock(_textView.TextBuffer, _position),
                    PositionAffinity.Successor);
            }
        }

        public double Right
        {
            get { return 0; }
        }

        public double Top
        {
            get { return 0; }
        }

        public double Width
        {
            get { return 1; }
        }

        public void EnsureVisible()
        {
        }

        public CaretPosition MoveTo(VirtualSnapshotPoint bufferPosition)
        {
            return MoveTo(bufferPosition, PositionAffinity.Successor);
        }

        public CaretPosition MoveTo(SnapshotPoint bufferPosition)
        {
            return MoveTo(bufferPosition, PositionAffinity.Successor);
        }

        public CaretPosition MoveTo(ITextViewLine textLine)
        {
            return MoveTo(new SnapshotPoint(textLine.Snapshot, textLine.Start));
        }

        public CaretPosition MoveTo(SnapshotPoint bufferPosition, PositionAffinity caretAffinity)
        {
            _position = bufferPosition.Position;

            return new CaretPosition(new VirtualSnapshotPoint(bufferPosition),
                  new MappingPointMock(bufferPosition.Snapshot.TextBuffer, bufferPosition.Position),
                  caretAffinity);
        }

        public CaretPosition MoveTo(VirtualSnapshotPoint bufferPosition, PositionAffinity caretAffinity)
        {
            _position = bufferPosition.Position.Position;

            return new CaretPosition(bufferPosition,
                  new MappingPointMock(bufferPosition.Position.Snapshot.TextBuffer, bufferPosition.Position),
                  PositionAffinity.Successor);
        }

        public CaretPosition MoveTo(ITextViewLine textLine, double xCoordinate)
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveTo(VirtualSnapshotPoint bufferPosition, PositionAffinity caretAffinity, bool captureHorizontalPosition)
        {
            return MoveTo(bufferPosition, caretAffinity);
        }

        public CaretPosition MoveTo(SnapshotPoint bufferPosition, PositionAffinity caretAffinity, bool captureHorizontalPosition)
        {
            return MoveTo(bufferPosition, caretAffinity);
        }

        public CaretPosition MoveTo(ITextViewLine textLine, double xCoordinate, bool captureHorizontalPosition)
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveToNextCaretPosition()
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveToPreferredCoordinates()
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveToPreviousCaretPosition()
        {
            throw new NotImplementedException();
        }

#pragma warning disable 0067
        public event EventHandler<CaretPositionChangedEventArgs> PositionChanged;
    }
}
