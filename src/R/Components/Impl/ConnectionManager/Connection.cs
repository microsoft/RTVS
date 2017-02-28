// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Security;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.R.Components.ConnectionManager {
    internal class Connection : ConnectionInfo, IConnection {
        public static Connection Create(ISecurityService securityService, IConnectionInfo connectionInfo) 
            => Create(securityService, connectionInfo.Name, connectionInfo.Path, connectionInfo.RCommandLineArguments, connectionInfo.IsUserCreated);

        public static Connection Create(ISecurityService securityService, string name, string path, string rCommandLineArguments, bool isUserCreated) {
            var brokerConnectionInfo = BrokerConnectionInfo.Create(securityService, name, path, rCommandLineArguments);
            return new Connection(brokerConnectionInfo, path, isUserCreated);
        }

        private Connection(BrokerConnectionInfo brokerConnectionInfo, string path, bool isUserCreated) 
            : base(brokerConnectionInfo.Name, path, brokerConnectionInfo.RCommandLineArguments, isUserCreated) {
            BrokerConnectionInfo = brokerConnectionInfo;
        }

        public BrokerConnectionInfo BrokerConnectionInfo { get; }
        public Uri Uri => BrokerConnectionInfo.Uri;
        public bool IsRemote => BrokerConnectionInfo.IsRemote;
    }
}