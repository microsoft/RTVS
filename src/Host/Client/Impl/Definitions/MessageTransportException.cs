// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Host.Client {
     internal class MessageTransportException : Exception {
        public MessageTransportException() {
        }

        public MessageTransportException(string message)
            : base(message) {
        }

        public MessageTransportException(string message, Exception innerException)
            : base(message, innerException) {
        }

        public MessageTransportException(Exception innerException)
            : this(innerException.Message, innerException) {
        }
    }
}
