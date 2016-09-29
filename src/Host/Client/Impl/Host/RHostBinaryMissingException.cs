// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Host.Client {
    [Serializable]
    public sealed class RHostBrokerBinaryMissingException : Exception {
        public RHostBrokerBinaryMissingException(string message = "Microsoft.R.Host.Broker.exe is missing")
            : base(message) {
        }
    }
}
