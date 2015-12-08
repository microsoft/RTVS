namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Range of integers
    /// </summary>
    public struct Range {
        int _end;

        public Range(int start, int count) {
            Start = start;
            Count = count;
            _end = start + count - 1;
        }

        public int Start { get; }

        public int Count { get; }

        public bool Contains(int value) {
            return (value >= Start) && (value <= _end);
        }

        public Range MoveStartBy(int count) {
            return new Range(Start + count, Count - count);
        }

        public Range MoveEndBy(int count) {
            return new Range(Start, Count + count);
        }
    }
}
