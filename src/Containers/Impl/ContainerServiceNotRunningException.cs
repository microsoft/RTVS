// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Containers {
    public class ContainerServiceNotRunningException : ContainerException {
        public ContainerServiceNotRunningException(string serviceName) {
            ServiceName = serviceName;
        }

        public ContainerServiceNotRunningException(string serviceName, string message) : base(message) {
            ServiceName = serviceName;
        }

        public ContainerServiceNotRunningException(string serviceName, string message, Exception innerException) : base(message, innerException) {
            ServiceName = serviceName;
        }

        public string ServiceName { get; }
    }
}
