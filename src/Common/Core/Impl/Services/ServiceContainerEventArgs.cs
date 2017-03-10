// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Common.Core.Services {
    [ExcludeFromCodeCoverage]
    public class ServiceContainerEventArgs : EventArgs {
        public Type ServiceType { get; }

        public ServiceContainerEventArgs(Type type) {
            ServiceType = type;
        }
    }
}
