using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Support.Utility;

namespace Microsoft.R.Support.Engine {
    /// <summary>
    /// An object that represents pending response from the R engine
    /// that is running in a separate process
    /// </summary>
    public class EngineResponse : AsyncData<object>, IDisposable {
        /// <summary>
        /// R engine process
        /// </summary>
        private Process _process;

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
        private object _objectLock = new object();
        private Task _heartBeatTask;
        private DateTime _lastResponseTime;

        /// <summary>
        /// Request creation time
        /// </summary>
        public DateTime CreationTime { get; private set; } = DateTime.Now;

        public EngineResponse(Process process, Action<object> dataReadyCallBack, Func<string, object, object> dataConverter, object p = null) :
            this(process, dataReadyCallBack) {
            _dataConverter = dataConverter;
            _parameter = p;
        }

        public EngineResponse(Process process, Action<object> dataReadyCallBack) :
            base(dataReadyCallBack) {
            _process = process;
            _process.OutputDataReceived += Process_OutputDataReceived;
            _process.ErrorDataReceived += Process_ErrorDataReceived;
        }

        public EngineResponse(object data) :
            base(data) {
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            lock (_objectLock) {
                // We want to collect full output, not just one line
                if (e != null && e.Data != null) {
                    _sb.AppendLine(e.Data);
                    _lastResponseTime = DateTime.Now;
                }

                if (_heartBeatTask == null) {
                    _heartBeatTask = Task.Run(async () => {
                        while ((DateTime.Now - _lastResponseTime).TotalMilliseconds < 200) {
                            await Task.Delay(50);
                        }
                        Disconnect();
                    });
                }
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e) {
        }

        private void Disconnect() {
            DisconnectFromEvents();

            object data;
            if (_dataConverter != null) {
                data = _dataConverter(_sb.ToString(), _parameter);
            } else {
                data = _sb.ToString();
            }

            _sb = null;

            SetData(data);
            Debug.WriteLine("R engine response time: {0} ms", (DateTime.Now - CreationTime).TotalMilliseconds);
        }

        private void DisconnectFromEvents() {
            if (_process != null) {
                // Usually called when request is canceled when user
                // moves mouse away from the function and VS quick info
                // tooltip session now is requesting information on 
                // a different function.
                _process.OutputDataReceived -= Process_OutputDataReceived;
                _process.ErrorDataReceived -= Process_ErrorDataReceived;
                _process = null;
            }
        }

        public void Dispose() {
            lock (_objectLock) {
                DisconnectFromEvents();
            }
        }
    }
}
