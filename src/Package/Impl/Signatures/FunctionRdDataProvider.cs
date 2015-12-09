using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.R.Debugger;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.VisualStudio.R.Package.DataInspect;

namespace Microsoft.VisualStudio.R.Package.Signatures {
    /// <summary>
    /// Provides RD data (help) on a function from the specified package.
    /// </summary>
    [Export(typeof(IFunctionRdDataProvider))]
    internal sealed class FunctionRdDataProvider : IFunctionRdDataProvider {
        private const int _sessionId = 73425;

        [Import]
        private IDebugSessionProvider DebugSessionProvider { get; set; }

        [Import]
        private IRSessionProvider SessionProvider { get; set; }

        private IRSession _session;
        private DebugSession _debugSession;

        /// <summary>
        /// Asynchronously fetches RD data on the function from R.
        /// When RD data is available, invokes specified callback
        /// passing funation name and the RD data extracted from R.
        /// </summary>
        public void GetFunctionRdData(string functionName, string packageName, Action<string> rdDataAvailableCallback) {
            Task.Run(async () => {
                DebugSession session = await DebugSessionProvider.GetDebugSessionAsync(SessionProvider.Current);
                var stackFrames = await session.GetStackFramesAsync();
                var globalStackFrame = stackFrames.FirstOrDefault(s => s.IsGlobal);
                if (globalStackFrame != null) {
                    string command = GetCommandText(functionName, packageName);
                    DebugEvaluationResult result = await globalStackFrame.EvaluateAsync(command, ".rtvs.signature", DebugEvaluationResultFields.ReprToString);
                    if (result is DebugValueEvaluationResult) {
                        rdDataAvailableCallback(((DebugValueEvaluationResult)result).Representation.Str);
                    }
                }
            });
        }

        public void Dispose() {
            if(_session != null) {
                _session.Disposed -= OnSessionDisposed;
                _session.Dispose();
                _session = null;
            }
        }

        private async Task CreateSessionAsync() {
            _session = SessionProvider.Create(_sessionId);
            _session.Disposed += OnSessionDisposed;

            _debugSession = await DebugSessionProvider.GetDebugSessionAsync(SessionProvider.Current);
        }

        private void OnSessionDisposed(object sender, EventArgs e) {
            if (_session != null) {
                _session.Disposed -= OnSessionDisposed;
                _session = null;
            }
            _debugSession = null;
        }

        private string GetCommandText(string functionName, string packageName) {
            if (string.IsNullOrEmpty(packageName)) {
                return ".rtvs.signature.help1('" + functionName + "')";
            }
            return ".rtvs.signature.help2('" + functionName + "', '" + packageName + "')";
        }
    }
}
