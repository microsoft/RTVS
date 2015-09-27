using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client
{
    public interface IRCallbacks
    {
        Task Connected(string rVersion);
        Task Disconnected();
        Task<string> ReadConsole(IReadOnlyCollection<IRContext> contexts, IRExpressionEvaluator evaluator, string prompt, string buf, int len, bool addToHistory);
        Task WriteConsoleEx(IReadOnlyCollection<IRContext> contexts, IRExpressionEvaluator evaluator, string buf, OutputType otype);
        Task ShowMessage(IReadOnlyCollection<IRContext> contexts, IRExpressionEvaluator evaluator, string s);
        Task<YesNoCancel> YesNoCancel(IReadOnlyCollection<IRContext> contexts, IRExpressionEvaluator evaluator, string s);
        Task Busy(IReadOnlyCollection<IRContext> contexts, IRExpressionEvaluator evaluator, bool which);
    }
}
