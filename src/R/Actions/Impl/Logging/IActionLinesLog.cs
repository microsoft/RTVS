using System.Collections.Generic;

namespace Microsoft.R.Actions.Logging {
    /// <summary>
    /// Log that can be read as text lines
    /// </summary>
    public interface IActionLinesLog : IActionLog {
        IReadOnlyList<string> Lines { get; }
    }
}
