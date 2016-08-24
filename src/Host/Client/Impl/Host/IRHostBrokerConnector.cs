// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Host.Client.Host {
    public interface IRHostBrokerConnector : IRHostConnector {
        event EventHandler BrokerChanged;
        
        void SwitchToLocalBroker(string name, string rBasePath = null, string rHostDirectory = null);
        void SwitchToRemoteBroker(Uri uri);
    }
}