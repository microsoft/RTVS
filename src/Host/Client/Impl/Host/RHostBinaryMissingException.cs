using System;

namespace Microsoft.R.Host.Client {
    [Serializable]
    public sealed class RHostBinaryMissingException : Exception {
        public RHostBinaryMissingException(string message = "Microsoft.R.Host.exe is missing")
            : base(message) {
        }
    }
}
