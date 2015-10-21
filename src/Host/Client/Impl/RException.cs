using System;

namespace Microsoft.R.Host.Client {
    public class RException : Exception {
        public RException(string message) : base(message) {
        }
    }
}