using System.Collections.Generic;

namespace Microsoft.R.Actions.Logging {
    /// <summary>
    /// Implementation of a text log that has multiple text lines.
    /// </summary>
    public class LinesLog : StringLog, IActionLinesLog {
        private readonly char[] _lineBreaks = { '\n' };
        private List<string> _lines;

        public IReadOnlyList<string> Lines {
            get {
                if (_lines == null) {
                    _lines = new List<string>();
                    _lines.AddRange(Content.Replace("\r", string.Empty).Split(_lineBreaks));
                }

                return _lines;
            }
        }

        public LinesLog(IActionLogWriter logWriter) : base(logWriter) { }
    }
}
