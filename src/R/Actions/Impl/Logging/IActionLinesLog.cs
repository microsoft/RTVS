using System.Collections.Generic;

namespace Microsoft.R.Actions.Logging
{
    public interface IActionLinesLog: IActionLog
    {
        IReadOnlyList<string> Lines { get; }
    }
}
