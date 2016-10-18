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

    [Serializable]
    public class InstanceDisposedException<T> : ObjectDisposedException {
        public T Instance { get; }

        public InstanceDisposedException(T instance) : base(GetObjectName(instance)) {
            Instance = instance;
        }

        public InstanceDisposedException(T instance, string message) : base(GetObjectName(instance), message) {
            Instance = instance;
        }

        public InstanceDisposedException(string message, Exception innerException) : base(message, innerException) {}
        protected InstanceDisposedException(SerializationInfo info, StreamingContext context) : base(info, context) {}

        private static string GetObjectName(T instance) {
            return instance?.GetType().Name ?? typeof(T).Name;
        }
    }
}