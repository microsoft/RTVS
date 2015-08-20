using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.R.Support.Utility;

namespace Microsoft.R.Support.Engine
{
    /// <summary>
    /// An object that represents pending response from the R engine
    /// that is running in a separate process
    /// </summary>
    public class EngineResponse : AsyncData<object>
    {
        /// <summary>
        /// R engine process
        /// </summary>
        private Process _process;

        /// <summary>
        /// Timestamp of the lst received data from the R engine process.
        /// Events are fired line by line and we need to collect all lines
        /// before passing data the converter. Thus there is a heardbeat
        /// task that determines if we haven't been receiving data for 
        /// a while so we can consider that all output was collected.
        /// </summary>
        private DateTime? _lastOutputReceived;

        /// <summary>
        /// Optional data converter that can transform data
        /// (such as parse RD response into a FunctionInfo)
        /// before setting data on the async object.
        /// </summary>
        private Func<string, object, object> _dataConverter;

        /// <summary>
        /// Any user parameter to pass to the data converter.
        /// </summary>
        private object _parameter;

        private StringBuilder _sb = new StringBuilder();
        private DateTime _processingStartTime = DateTime.Now;

        public EngineResponse(Process process, Action<object> dataReadyCallBack, Func<string, object, object> dataConverter, object p = null) :
            this(process, dataReadyCallBack)
        {
            _dataConverter = dataConverter;
            _parameter = p;
        }

        public EngineResponse(Process process, Action<object> dataReadyCallBack) :
            base(dataReadyCallBack)
        {
            _process = process;
            _process.OutputDataReceived += Process_OutputDataReceived;
        }

        public EngineResponse(object data) :
            base(data)
        {
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

            object data;
            if (_dataConverter != null)
            {
                data = _dataConverter(_sb.ToString(), _parameter);
            }
            else
            {
                data = _sb.ToString();
            }

            _sb = null;

            SetData(data);
            Debug.WriteLine("R engine response time: {0} ms", (DateTime.Now - _processingStartTime).TotalMilliseconds);
        }
    }
}
