// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Host.Client.Host {
    public interface IRHostBrokerConnector : IRHostConnector {
        /// <summary>
        /// String token that allows to identify broker that is used to connect to RHost
        /// For local connections, it is a path to the RHost
        /// For remote connections, it is a URL
        /// </summary>
        string BrokerId { get; }

        event EventHandler BrokerIdChanged;

        void SwitchToLocalBroker(string rBasePath, string rHostDirectory = null);
    }
}