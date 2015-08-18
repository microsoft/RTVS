using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Support.Engine;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.RD.Parser;

namespace Microsoft.R.Support.Help
{
    public sealed class RHelpDataSource : IRHelpDataSource
    {
        private EngineSession _session;
        private Dictionary<string, EngineResponse> _pendingRequests;

        // TODO: we need some lifetime management here since 
        // technically packages can get loaded and unloaded so
        // a function with the same name but different parameters 
        // may end up loaded while signature help will still be
        // showing stale data. On the other hand this is probably
        // quite rare case.
        private Dictionary<string, FunctionInfo> _functions;

        public RHelpDataSource()
        {
            _session = new EngineSession(Rd2FunctionInfoConverter);
            _functions = new Dictionary<string, FunctionInfo>();
            _pendingRequests = new Dictionary<string, EngineResponse>();
        }

        public async Task<EngineResponse> GetFunctionHelp(string functionName, string packageName)
        {
            FunctionInfo functionInfo;
            EngineResponse response;

            if (_functions.TryGetValue(functionName, out functionInfo))
            {
                return new EngineResponse(functionInfo);
            }

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

            response = await _session.SendCommand(command, functionName);
            _pendingRequests[functionName] = response;

            return response;
        }

        public void Dispose()
        {
            if (_session != null)
            {
                _session.Dispose();
            }
        }

        private object Rd2FunctionInfoConverter(string rdData, object parameter)
        {
            FunctionInfo info = RdParser.GetFunctionInfo(parameter as string, rdData);
            _functions[info.Name] = info;

            EditorShell.DispatchOnUIThread(() => _pendingRequests.Remove(info.Name));
            return info;
        }
    }
}
