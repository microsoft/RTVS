// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client.BrokerServices;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Client.Host {
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public class RHostDisconnectedException : OperationCanceledException {
        public RHostDisconnectedException() : this(Resources.RHostDisconnected) { }

        public RHostDisconnectedException(string message) : base(message) { }

        public RHostDisconnectedException(string message, Exception innerException) : base(message, innerException) { }

        public RHostDisconnectedException(CancellationToken token) : base(token) { }

        public RHostDisconnectedException(string message, CancellationToken token) : base(message, token) { }

        public RHostDisconnectedException(string message, Exception innerException, CancellationToken token) : base(message, innerException, token) { }

        public RHostDisconnectedException(BrokerApiErrorException ex) : base(FromBrokerApiException(ex), ex) { }

        protected RHostDisconnectedException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        private static string FromBrokerApiException(BrokerApiErrorException ex) {
            switch (ex.ApiError) {
                case BrokerApiError.NoRInterpreters:
                    return Resources.Error_NoRInterpreters;
                case BrokerApiError.InterpreterNotFound:
                    return Resources.Error_InterpreterNotFound;
                case BrokerApiError.UnableToStartRHost:
                    if (!string.IsNullOrEmpty(ex.Message)) {
                        return Resources.Error_UnableToStartHostException.FormatInvariant(ex.Message);
                    }
                    return Resources.Error_UnknownError;
                case BrokerApiError.PipeAlreadyConnected:
                    return Resources.Error_PipeAlreadyConnected;
                case BrokerApiError.Win32Error:
                    if (!string.IsNullOrEmpty(ex.Message)) {
                        return Resources.Error_BrokerWin32Error.FormatInvariant(ex.Message);
                    }
                    return Resources.Error_BrokerUnknownWin32Error;
            }

            Debug.Fail("No localized resources for broker API error" + ex.ApiError.ToString());
            return ex.ApiError.ToString();
        }
    }
}
