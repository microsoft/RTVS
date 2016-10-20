using System;
using System.Runtime.Serialization;

namespace Microsoft.Common.Core.Exceptions {
    /// <summary>
    /// An exception that should not be silently handled and logged.
    /// </summary>
    [Serializable]
    public class CriticalException : Exception {
        public CriticalException() { }
        public CriticalException(string message) : base(message) { }
        public CriticalException(string message, Exception inner) : base(message, inner) { }
        protected CriticalException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}