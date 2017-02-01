// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Languages.Editor.Services {
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
