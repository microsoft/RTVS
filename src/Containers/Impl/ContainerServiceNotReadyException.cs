// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Containers {
    public class ContainerServiceNotReadyException : ContainerException {
        public ContainerServiceNotReadyException() { }

        public ContainerServiceNotReadyException(string message) : base(message) { }

        public ContainerServiceNotReadyException(string message, Exception innerException) : base(message, innerException) { }
    }
}
