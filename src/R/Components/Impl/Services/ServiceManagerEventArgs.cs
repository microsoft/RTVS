using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.R.Components.Services {
    [ExcludeFromCodeCoverage]
    public class ServiceManagerEventArgs : EventArgs {
        public object Service { get; }
        public Type ServiceType { get; }

        public ServiceManagerEventArgs(Type type, object service) {
            Service = service;
            ServiceType = type;
        }

        public ServiceManagerEventArgs(Type type)
            : this(type, null) { }
    }
}
