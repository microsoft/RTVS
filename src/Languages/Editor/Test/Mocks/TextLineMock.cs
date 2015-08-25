using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Test.Mocks
{
    /// <summary>
    /// Mock implementation of ITextSnapshotLine based on mock snapshot for unit tests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TextLineMock : ITextSnapshotLine
    {
        private int _lineNumber;
        private ITextSnapshot _snapshot;
        private int _start;
        private int _length;

        public TextLineMock(ITextSnapshot snapshot, int start, int length, int lineNumber)
        {
            _snapshot = snapshot;
            _start = start;
            _length = length;
            _lineNumber = lineNumber;
        }

        #region ITextSnapshotLine Members

        public SnapshotPoint End
        {
            get { return new SnapshotPoint(_snapshot, _start + _length); }
        }

        public SnapshotPoint EndIncludingLineBreak
        {
            get
            {
                return
                    _start + _length + 2 <= _snapshot.Length ?
                    new SnapshotPoint(_snapshot, _start + _length + 2) :
                    new SnapshotPoint(_snapshot, _start + _length);
            }
        }

        public SnapshotSpan Extent
        {
            get { return new SnapshotSpan(_snapshot, new Span(_start, _length)); }
        }

        public SnapshotSpan ExtentIncludingLineBreak
        {
            get
            {
                return
                    _start + _length + 2 <= _snapshot.Length ?
                    new SnapshotSpan(_snapshot, new Span(_start, _length + 2)) :
                    new SnapshotSpan(_snapshot, new Span(_start, _length));
            }
        }

        public string GetLineBreakText()
        {
            return "\r\n";
        }

        public string GetText()
        {
            return _snapshot.GetText(_start, _length);
        }

        public string GetTextIncludingLineBreak()
        {
            return GetText() + GetLineBreakText();
        }

        public int Length
        {
            get { return _length; }
        }

        public int LengthIncludingLineBreak
        {
            get { return _start + _length + 2 <= _snapshot.Length ? _length + 2 : _length; }
        }

        public int LineBreakLength
        {
            get { return _start + _length + 2 <= _snapshot.Length ? 2 : 0; }
        }

        public int LineNumber
        {
            get { return _lineNumber; }
        }

        public ITextSnapshot Snapshot
        {
            get { return _snapshot; }
        }

        public SnapshotPoint Start
        {
            get { return new SnapshotPoint(_snapshot, _start); }
        }

        #endregion
    }
}
