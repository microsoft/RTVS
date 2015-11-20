using System;
using System.Diagnostics;
using Microsoft.R.Support.Engine;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.RD.Parser;

namespace Microsoft.R.Support.Help.Functions {
    internal sealed class RdFunctionHelp : IDisposable {
        private EngineSession _session;
        private string _currentFunctionName;
        private EngineResponse _pendingResponse;
        private int _retryCount;

        public RdFunctionHelp() {
            _session = new EngineSession(Rd2FunctionInfoConverter);
        }

        public void GetFunctionRdHelp(string functionName, string packageName, Action<object> dataReadyCallback) {
            try {
                if (_pendingResponse != null && _retryCount < 3) {
                    if (_currentFunctionName == functionName) {
                        return;
                    }
                    _pendingResponse.Dispose();
                    _pendingResponse = null;

                    _retryCount++;
                    if (_retryCount > 3) {
                        _session.Dispose();
                        _session = new EngineSession(Rd2FunctionInfoConverter);
                    }
                }

                string command = "x <- help(\"" + functionName;
                if (string.IsNullOrEmpty(packageName)) {
                    command += "\");";
                } else {
                    command += "\", \"" + packageName + "\");";
                }

                command += " utils:::.getHelpFile(x)";

                _pendingResponse = _session.SendCommand(command, functionName, dataReadyCallback);
                _currentFunctionName = functionName;
            } catch (Exception) {
                if (_pendingResponse != null) {
                    _pendingResponse.Dispose();
                    _pendingResponse = null;
                    _currentFunctionName = null;
                }
            }
        }

        public void Dispose() {
            if (_session != null) {
                _session.Dispose();
                _session = null;
            }
        }

        private object Rd2FunctionInfoConverter(string rdData, object parameter) {
            string functionName = parameter as string;
            IFunctionInfo info = null;
            _retryCount = 0;

            try {
                info = RdParser.GetFunctionInfo(functionName, rdData);
            } catch (Exception ex) {
                Debug.WriteLine("Exception in parsing R engine RD response: {0}", ex.Message);
            } finally {
                if (_pendingResponse != null) {
                    _pendingResponse.Dispose();
                    _pendingResponse = null;
                    _currentFunctionName = null;
                }
            }

            return info;
        }
    }
}
