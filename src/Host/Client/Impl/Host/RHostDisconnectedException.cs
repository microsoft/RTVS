// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;

namespace Microsoft.R.Host.Client.Host {
    [System.Runtime.InteropServices.ComVisible(true)]
    public class RHostDisconnectedException : OperationCanceledException {
        public RHostDisconnectedException() : this(Resources.RHostDisconnected) { }

        public RHostDisconnectedException(string message) : base(message) { }

        public RHostDisconnectedException(string message, Exception innerException) : base(message, innerException) { }

        public RHostDisconnectedException(CancellationToken token) : base(token) { }

        public RHostDisconnectedException(string message, CancellationToken token) : base(message, token) { }

        public RHostDisconnectedException(string message, Exception innerException, CancellationToken token) : base(message, innerException, token) { }

        //protected RHostDisconnectedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
