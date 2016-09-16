// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Client.Host {
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public class RHostDisconnectedException : OperationCanceledException {
        public RHostDisconnectedException() : this(Resources.RHostDisconnected) { }

        public RHostDisconnectedException(string message) : base(message) {}

        public RHostDisconnectedException(string message, Exception innerException) : base(message, innerException) {}

        public RHostDisconnectedException(CancellationToken token) : base(token) {}

        public RHostDisconnectedException(string message, CancellationToken token): base(message, token) {}

        public RHostDisconnectedException(string message, Exception innerException, CancellationToken token) : base(message, innerException, token) {}

        public RHostDisconnectedException(BrokerApiError error) : base(FromBrokerApiError(error)) { }

        protected RHostDisconnectedException(SerializationInfo info, StreamingContext context) : base (info, context) {}

        private static string FromBrokerApiError(BrokerApiError error) {
            switch (error) {
                case BrokerApiError.NoRInterpreters:
                    return Resources.Error_NoRInterpreters;
                case BrokerApiError.InterpreterNotFound:
                    return Resources.Error_InterpreterNotFound;
            }

            Debug.Fail("No localized resources for broker API error" + error.ToString());
            return error.ToString();
        }
    }
}
