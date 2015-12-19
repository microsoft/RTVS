using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Host.Client.Signatures {
    /// <summary>
    /// Provides RD data (help) on a function from the specified package.
    /// </summary>
    [Export(typeof(IFunctionRdDataProvider))]
    internal sealed class FunctionRdDataProvider : IFunctionRdDataProvider {
        private const int _sessionId = 73425;
        private IRSession _session;

        /// <summary>
        /// Asynchronously fetches RD data on the function from R.
        /// When RD data is available, invokes specified callback
        /// passing funation name and the RD data extracted from R.
        /// </summary>
        public void GetFunctionRdData(string functionName, string packageName, Action<string> rdDataAvailableCallback) {
            Task.Run(async () => {
                await CreateSessionAsync();
                using (var eval = await _session.BeginEvaluationAsync()) {
                    string command = GetCommandText(functionName, packageName);
                    REvaluationResult result = await eval.EvaluateAsync(command);
                    if (result.ParseStatus == RParseStatus.OK && result.StringResult != null && result.StringResult.Length > 2) {
                        rdDataAvailableCallback(result.StringResult);
                    }
                }
            });
        }

        public void Dispose() {
            if (_session != null) {
                _session.Disposed -= OnSessionDisposed;
                _session.Dispose();
                _session = null;
            }
        }

        private async Task CreateSessionAsync() {
            if (_session == null) {
                var provider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                _session = provider.Create(_sessionId, null);
                _session.Disposed += OnSessionDisposed;
                await _session.StartHostAsync("RdData", IntPtr.Zero);
            }
        }

        private void OnSessionDisposed(object sender, EventArgs e) {
            if (_session != null) {
                _session.Disposed -= OnSessionDisposed;
                _session = null;
            }
        }

        private string GetCommandText(string functionName, string packageName) {
            if (string.IsNullOrEmpty(packageName)) {
                return ".rtvs.signature.help1('" + functionName + "')";
            }
            return ".rtvs.signature.help2('" + functionName + "', '" + packageName + "')";
        }
    }
}
