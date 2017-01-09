// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.R.Components.ConnectionManager {
    internal class Connection : ConnectionInfo, IConnection {
        public Connection(IConnectionInfo connectionInfo) 
            : base(connectionInfo) {
            BrokerConnectionInfo = BrokerConnectionInfo.Create(connectionInfo.Name, Path, RCommandLineArguments);
        }

        public Connection(string name, string path, string rCommandLineArguments, bool isUserCreated) 
            : base(name, path, rCommandLineArguments, isUserCreated) {
            BrokerConnectionInfo = BrokerConnectionInfo.Create(name, path, rCommandLineArguments);
        }

        public BrokerConnectionInfo BrokerConnectionInfo { get; }

        public Uri Uri => BrokerConnectionInfo.Uri;
        public bool IsRemote => BrokerConnectionInfo.IsRemote;
    }
}