// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;

namespace Microsoft.R.Containers {
    public class ContainerException : OperationCanceledException {
        public ContainerException() { }

        public ContainerException(string message) : base(message) { }

        public ContainerException(string message, Exception innerException) : base(message, innerException) { }

        public ContainerException(CancellationToken token) : base(token) { }

        public ContainerException(string message, CancellationToken token) : base(message, token) { }

        public ContainerException(string message, Exception innerException, CancellationToken token) : base(message, innerException, token) { }

        //protected RHostDisconnectedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
