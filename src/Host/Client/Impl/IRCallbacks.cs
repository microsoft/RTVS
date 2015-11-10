using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRCallbacks {
        Task Connected(string rVersion);
        Task Disconnected();

        Task<YesNoCancel> YesNoCancel(IReadOnlyList<IRContext> contexts, string s, bool isEvaluationAllowed, CancellationToken ct);
        Task<string> ReadConsole(IReadOnlyList<IRContext> contexts, string prompt, int len, bool addToHistory, bool isEvaluationAllowed, CancellationToken ct);

        Task WriteConsoleEx(string buf, OutputType otype, CancellationToken ct);
        Task ShowMessage(string s, CancellationToken ct);
        Task Busy(bool which, CancellationToken ct);
        Task PlotXaml(string xamlFilePath, CancellationToken ct);

        /// <summary>
        /// Tracks change of current directory in R session command line.
        /// R Host pushes new directory to VS so it can correctly display 
        /// the directory name in the REPL window toolbar.
        /// </summary>
        Task SetCurrentDirectory(string directory);

        /// <summary>
        /// Asks VS to open specified URL in the help window browser
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task ShowHelp(string url);
    }
}
