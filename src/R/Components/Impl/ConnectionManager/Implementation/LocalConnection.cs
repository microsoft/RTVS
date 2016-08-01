// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.R.Components.ConnectionManager.Implementation {
    internal class LocalConnection : IConnection {
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRHostBrokerConnector _brokerConnector;

        public LocalConnection(string name, string rBasePath, DateTime timeStamp, IRSessionProvider sessionProvider, IRHostBrokerConnector brokerConnector) {
            Id = new Uri(rBasePath);
            Name = name;
            RBasePath = rBasePath;
            TimeStamp = timeStamp;
            _sessionProvider = sessionProvider;
            _brokerConnector = brokerConnector;
        }

        public Uri Id { get; }
        public string Name { get; }
        public string RBasePath { get; }
        public DateTime TimeStamp { get; }

        public async Task ConnectAsync() {
            _brokerConnector.SwitchToLocalBroker(RBasePath);
            await _sessionProvider.RestartSessions(_brokerConnector);
        }
    }
}