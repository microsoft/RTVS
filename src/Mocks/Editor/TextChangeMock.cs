using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Editor.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class TextChangeMock : ITextChange, IComparable
    {
        public int Start;
        public int OldLength;
        public string NewText;
        public string OldText;

        public TextChangeMock()
            : this(0, 0, String.Empty, String.Empty)
        {
        }
        public TextChangeMock(int start, int oldLength, string newText)
            : this(start, oldLength, String.Empty, newText)
        {
        }

        public TextChangeMock(int start, int oldLength, string oldText, string newText)
        {
            Start = start;
            OldLength = oldLength;
            OldText = oldText;
            NewText = newText;
        }

        #region ITextChange Members

        public int Delta
        {
            get { return NewText.Length - OldLength; }
        }

        public int LineCountDelta
        {
            get { return 0; }
        }

        public int NewEnd
        {
            get { return Start + NewText.Length; }
        }

        public int NewLength
        {
            get { return NewText.Length; }
        }

        public int NewPosition
        {
            get { return Start; }
        }

        public Span NewSpan
        {
            get { return new Span(Start, NewLength); }
        }

        string ITextChange.NewText
        {
            get { return NewText; }
        }

        public int OldEnd
        {
            get { return Start + OldLength; }
        }

        int ITextChange.OldLength
        {
            get { return OldLength; }
        }

        public int OldPosition
        {
            get { return Start; }
        }

        public Span OldSpan
        {
            get { return new Span(Start, OldLength); }
        }

        string ITextChange.OldText
        {
            get { return OldText; }
        }

        #endregion

        #region IComparable Members
        public int CompareTo(object obj)
        {
            var other = obj as TextChangeMock;
            return this.OldPosition.CompareTo(other.OldPosition);
        }
        #endregion
    }

}
