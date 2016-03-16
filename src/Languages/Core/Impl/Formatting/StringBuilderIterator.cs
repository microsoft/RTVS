using System.Text;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Formatting {
    public sealed class StringBuilderIterator : ITextIterator {
        private readonly StringBuilder _sb;

        public StringBuilderIterator(StringBuilder sb) {
            _sb = sb;
        }

        public char this[int position] {
            get {
                return position >= 0 && position < _sb.Length ? _sb[position] : '\0';
            }
        }

        public int Length {
            get {
                return _sb.Length;
            }
        }
    }
}
