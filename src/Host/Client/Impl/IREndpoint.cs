using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host {
    public interface IRCallbacks : IDisposable {
        Task Connected(string rVersion);
        Task Disconnected();
        Task<string> ReadConsole(IReadOnlyCollection<IRContext> contexts, string prompt, string buf, int len, bool addToHistory);
        Task WriteConsoleEx(IReadOnlyCollection<IRContext> contexts, string buf, OutputType otype);
        Task ShowMessage(IReadOnlyCollection<IRContext> contexts, string s);
        Task<YesNoCancel> YesNoCancel(IReadOnlyCollection<IRContext> contexts, string s);
        Task Busy(IReadOnlyCollection<IRContext> contexts, bool which);
    }

    public enum YesNoCancel {
        No = -1,
        Cancel = 0,
        Yes = 1
    }

    public enum OutputType {
        Output = 0,
        Error = 1
    }
}
