using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.Host.Client {
    public interface IRCallbacks {
        Task Connected(string rVersion);
        Task Disconnected();

        Task<MessageButtons> ShowDialog(IReadOnlyList<IRContext> contexts, string s, bool isEvaluationAllowed, MessageButtons buttons, CancellationToken ct);
        Task<string> ReadConsole(IReadOnlyList<IRContext> contexts, string prompt, int len, bool addToHistory, bool isEvaluationAllowed, CancellationToken ct);

        Task WriteConsoleEx(string buf, OutputType otype, CancellationToken ct);
        Task ShowMessage(string s, CancellationToken ct);
        Task Busy(bool which, CancellationToken ct);
        Task PlotXaml(string xamlFilePath, CancellationToken ct);

        /// <summary>
        /// Asks VS to open specified URL in the help window browser
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task Browser(string url);
    }
}
