using System;
using System.Threading.Tasks;
using Microsoft.R.Support.Engine;

namespace Microsoft.R.Support.Help
{
    public sealed class RHelpDataSource : IDisposable
    {
        EngineSession _session;

        public RHelpDataSource()
        {
            _session = new EngineSession();
        }

        public async Task<EngineResponse> GetFunctionHelp(string func, string package)
        {
            string command;

            if (string.IsNullOrEmpty(package))
            {
                command = "x <- help(\"" + func + "\");";
            }
            else
            {
                command = "x <- help(\"" + func + "\", \"" + package + "\");";
            }

            command += " utils:::.getHelpFile(x)";

            EngineResponse response = await _session.SendCommand(command);
            return response;
        }

        public void Dispose()
        {
            if (_session != null)
            {
                _session.Dispose();
            }
        }
    }
}
