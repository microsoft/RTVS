// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Host.Broker.Pipes {
    public class PipeDisconnectedException : Exception {
        public PipeDisconnectedException()
            : this("Host end of the message pipe was disconnected") {
        }

        public PipeDisconnectedException(string message)
            : base(message) {
        }
    }
}
