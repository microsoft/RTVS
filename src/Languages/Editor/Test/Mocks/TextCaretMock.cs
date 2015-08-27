using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace Microsoft.Languages.Editor.Test.Mocks
{
    [ExcludeFromCodeCoverage]
    public class TextCaretMock : ITextCaret
    {
        private int _position;

        public TextCaretMock(int position)
        {
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
            get { return new CaretPosition(); }
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
            throw new NotImplementedException();
        }

        public CaretPosition MoveTo(SnapshotPoint bufferPosition)
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveTo(ITextViewLine textLine)
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveTo(SnapshotPoint bufferPosition, PositionAffinity caretAffinity)
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveTo(VirtualSnapshotPoint bufferPosition, PositionAffinity caretAffinity)
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveTo(ITextViewLine textLine, double xCoordinate)
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveTo(VirtualSnapshotPoint bufferPosition, PositionAffinity caretAffinity, bool captureHorizontalPosition)
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveTo(SnapshotPoint bufferPosition, PositionAffinity caretAffinity, bool captureHorizontalPosition)
        {
            throw new NotImplementedException();
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
