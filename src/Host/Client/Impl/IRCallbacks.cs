using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRCallbacks {
        Task Connected(string rVersion);
        Task Disconnected();
        Task<string> ReadConsole(IReadOnlyCollection<IRContext> contexts, string prompt, string buf, int len, bool addToHistory, CancellationToken ct);
        Task WriteConsoleEx(IReadOnlyCollection<IRContext> contexts, string buf, OutputType otype, CancellationToken ct);
        Task ShowMessage(IReadOnlyCollection<IRContext> contexts, string s, CancellationToken ct);
        Task<YesNoCancel> YesNoCancel(IReadOnlyCollection<IRContext> contexts, string s, CancellationToken ct);
        Task Busy(IReadOnlyCollection<IRContext> contexts, bool which, CancellationToken ct);
        Task Evaluate(IReadOnlyCollection<IRContext> contexts, IRExpressionEvaluator evaluator, CancellationToken ct);
        Task PlotXaml(IReadOnlyCollection<IRContext> contexts, string xamlFilePath, CancellationToken ct);
    }
}
