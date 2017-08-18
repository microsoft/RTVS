// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Containers {
    public class ContainerServiceNotRunningException : ContainerException {
        public ContainerServiceNotRunningException() { }

        public ContainerServiceNotRunningException(string message) : base(message) { }

        public ContainerServiceNotRunningException(string message, Exception innerException) : base(message, innerException) { }
    }
}
