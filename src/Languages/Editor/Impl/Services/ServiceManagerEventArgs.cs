using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Languages.Editor.Services {
    [ExcludeFromCodeCoverage]
    public class ServiceManagerEventArgs : EventArgs {
        public object Service { get; private set; }
        public Type ServiceType { get; private set; }

        public ServiceManagerEventArgs(Type type, object service) {
            Service = service;
            ServiceType = type;
        }

        public ServiceManagerEventArgs(Type type)
            : this(type, null) { }
    }
}
