// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core.Services {
    public class ServiceContainerEventArgs : EventArgs {
        public Type ServiceType { get; }

        public ServiceContainerEventArgs(Type type) {
            ServiceType = type;
        }
    }
}
