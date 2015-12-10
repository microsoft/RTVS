using System;

namespace Microsoft.R.Host.Client {
    public class ROutputEventArgs : EventArgs {
        public OutputType OutputType { get; }
        public string Message { get; }

        public ROutputEventArgs(OutputType outputType, string message) {
            OutputType = outputType;
            Message = message;
        }
    }
}