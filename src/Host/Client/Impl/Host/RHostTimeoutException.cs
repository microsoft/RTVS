using System;

namespace Microsoft.R.Host.Client {
    [Serializable]
    public sealed class RHostTimeoutException : Exception {
        public RHostTimeoutException(string message)
            : base(message) {
        }
    }
}
