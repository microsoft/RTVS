// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Host.Client.Host {
    public interface IRHostBrokerConnector : IRHostConnector {
        Uri BrokerUri { get; }

        event EventHandler BrokerChanged;

        void SwitchToLocalBroker(string rBasePath, string rHostDirectory = null);
    }
}