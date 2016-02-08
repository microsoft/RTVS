using System;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class DefaultHeaderData : IRange<string> {
        public enum Mode {
            Column,
            Row,
        }

        private Mode _mode;
        public DefaultHeaderData(Range range, Mode columnMode) {
            Range = range;
            _mode = columnMode;
        }

        public string this[int index] {
            get {
                int rIndex = index + 1; // R index is 1-based
                if (_mode == Mode.Column) {
                    return string.Format("[,{0}]", rIndex);
                }
                return string.Format("[{0},]", rIndex);
            }

            set {
                throw new NotImplementedException();
            }
        }

        public Range Range { get; }
    }
}
