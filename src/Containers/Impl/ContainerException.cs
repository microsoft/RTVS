// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;

namespace Microsoft.R.Containers {
    public class ContainerException : Exception {
        public ContainerException() { }

        public ContainerException(string message) : base(message) { }

        public ContainerException(string message, Exception innerException) : base(message, innerException) { }
    }
}
