// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;

namespace Microsoft.R.Host.Client.Host {
    public static class RHostBrokerConnectorExtensions {
        public static bool IsRemoteConnection(this IRHostBrokerConnector brokerConnector) {
            // Any URI the does not start with 'file' is remote.
            return !brokerConnector.BrokerUri.Scheme.StartsWithIgnoreCase("file");
        }
    }
}
