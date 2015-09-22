using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client
{
    public interface IRCallbacks
    {
        Task Connected(string rVersion);
        Task Disconnected();
        Task<string> ReadConsole(IReadOnlyCollection<IRContext> contexts, string prompt, string buf, int len, bool addToHistory);
        Task WriteConsoleEx(IReadOnlyCollection<IRContext> contexts, string buf, OutputType otype);
        Task ShowMessage(IReadOnlyCollection<IRContext> contexts, string message, MessageSeverity severity);
        Task<YesNoCancel> YesNoCancel(IReadOnlyCollection<IRContext> contexts, string s);
        Task Busy(IReadOnlyCollection<IRContext> contexts, bool which);
    }
}
