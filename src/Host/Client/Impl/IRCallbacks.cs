using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRCallbacks {
        Task Connected(string rVersion);
        Task Disconnected();

        Task<YesNoCancel> YesNoCancel(IReadOnlyList<IRContext> contexts, string s, bool isEvaluationAllowed, CancellationToken ct);
        Task<string> ReadConsole(IReadOnlyList<IRContext> contexts, string prompt, string buf, int len, bool addToHistory, bool isEvaluationAllowed, CancellationToken ct);

        Task WriteConsoleEx(IReadOnlyList<IRContext> contexts, string buf, OutputType otype, CancellationToken ct);
        Task ShowMessage(IReadOnlyList<IRContext> contexts, string s, CancellationToken ct);
        Task Busy(IReadOnlyList<IRContext> contexts, bool which, CancellationToken ct);
        Task PlotXaml(IReadOnlyList<IRContext> contexts, string xamlFilePath, CancellationToken ct);
    }
}
