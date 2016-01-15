using System.Collections.Generic;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Range of integers
    /// </summary>
    public struct Range {
        int _end;

        public Range(int start, int count) {
            Start = start;
            Count = count;
            _end = start + count;
        }

        public int Start { get; }

        public int Count { get; }

        public bool Contains(int value) {
            return (value >= Start) && (value < _end);
        }

        public bool Contains(Range other) {
            if (Count == 0) return false;

            return (other.Start <= this.Start) && (other._end >= this._end);
        }

        public IEnumerable<int> GetEnumerable() {
            for (int i = Start; i < _end; i++) {
                yield return i;
            }
        }
    }
}
