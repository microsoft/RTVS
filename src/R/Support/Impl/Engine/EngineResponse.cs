using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Support.Utility;

namespace Microsoft.R.Support.Engine
{
    public sealed class EngineResponse : AsyncDataSource<string>
    {
        private Process _process;
        private StringBuilder _sb = new StringBuilder();
        private DateTime? _lastOutputReceived;

        public EngineResponse(Process process)
        {
            _process = process;
            _process.OutputDataReceived += Process_OutputDataReceived;
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // We want to collect full output, not just one line
            if (e != null && e.Data != null)
            {
                if (_lastOutputReceived == null)
                {
                    _lastOutputReceived = DateTime.Now;
                    Task.Run(() => HeartbeatThread());
                }

                _sb.AppendLine(e.Data);
            }
        }

        private void HeartbeatThread()
        {
            while (true)
            {
                TimeSpan ts = DateTime.Now - _lastOutputReceived.Value;
                if (ts.Milliseconds > 50)
                {
                    Disconnect();
                    break;
                }

                Task.Delay(20).Wait();
            }
        }

        private void Disconnect()
        {
            _process.OutputDataReceived -= Process_OutputDataReceived;
            _process = null;

            string data = _sb.ToString();
            _sb = null;

            SetData(data);
        }
    }
}
