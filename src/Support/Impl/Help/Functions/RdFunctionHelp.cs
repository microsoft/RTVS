using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.R.Support.Engine;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.RD.Parser;

namespace Microsoft.R.Support.Help.Functions
{
    internal sealed class RdFunctionHelp
    {
        private EngineSession _session;
        private ConcurrentDictionary<string, EngineResponse> _pendingRequests;

        public RdFunctionHelp()
        {
            _session = new EngineSession(Rd2FunctionInfoConverter);
            _pendingRequests = new ConcurrentDictionary<string, EngineResponse>();
        }

        public async Task<EngineResponse> GetFunctionRdHelp(string functionName, string packageName, Action<object> dataReadyCallback)
        {
            EngineResponse response;

            if (_pendingRequests.TryGetValue(functionName, out response))
            {
                return response;
            }

            string command = "x <- help(\"" + functionName;
            if (string.IsNullOrEmpty(packageName))
            {
                command += "\");";
            }
            else
            {
                command += "\", \"" + packageName + "\");";
            }
            command += " utils:::.getHelpFile(x)";

            response = await _session.SendCommand(command, functionName, dataReadyCallback);
            _pendingRequests[functionName] = response;

            return response;
        }

        public void Dispose()
        {
            if (_session != null)
            {
                _session.Dispose();
                _session = null;
            }
        }

        private object Rd2FunctionInfoConverter(string rdData, object parameter)
        {
            IFunctionInfo info = RdParser.GetFunctionInfo(parameter as string, rdData);
            EngineResponse value;

            _pendingRequests.TryRemove(info.Name, out value);
            return info;
        }
    }
}
