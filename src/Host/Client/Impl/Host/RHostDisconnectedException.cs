// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
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

        public RHostDisconnectedException(int code, Exception innerException) : this(code, innerException.Message) { }

        public RHostDisconnectedException(int code, string message) : base(FromCustomHttpErrorCode(code, message)) { }

        protected RHostDisconnectedException(SerializationInfo info, StreamingContext context) : base (info, context) {}

        private static string FromCustomHttpErrorCode(int code, string message) {
            switch ((CustomHttpError)code) {
                case CustomHttpError.NoRInterpreters:
                    return Resources.Error_NoRInterpreters;
                case CustomHttpError.InterpreterNotFound:
                    return Resources.Error_InterpreterNotFound;
            }
            return message;
        }
    }
}
