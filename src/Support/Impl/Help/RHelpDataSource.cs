using System;
using System.Threading.Tasks;

namespace Microsoft.R.Support.Help
{
    public sealed class RHelpDataSource : IDisposable
    {
        EngineHelpSession _session;

        public RHelpDataSource()
        {
            _session = new EngineHelpSession();
        }

        public async Task<string> GetHelpText(string func, string package)
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

            string result = await _session.SendCommand(command);

            return result;
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
