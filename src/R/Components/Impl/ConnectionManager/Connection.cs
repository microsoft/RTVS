// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Microsoft.Common.Core.Security;
using Microsoft.R.Containers;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.R.Components.ConnectionManager {
    public class Connection : ConnectionInfo, IConnection {
        public static Connection Create(ISecurityService securityService, IConnectionInfo connectionInfo, bool fetchHostLoad) {
            var brokerConnectionInfo = BrokerConnectionInfo.Create(securityService, connectionInfo.Name, connectionInfo.Path, connectionInfo.RCommandLineArguments, fetchHostLoad);
            return new Connection(brokerConnectionInfo, connectionInfo);
        }

        public static Connection Create(ISecurityService securityService, string name, string path, string rCommandLineArguments, bool isUserCreated, bool fetchHostLoad) {
            var brokerConnectionInfo = BrokerConnectionInfo.Create(securityService, name, path, rCommandLineArguments, fetchHostLoad);
            return new Connection(brokerConnectionInfo, path, isUserCreated, false);
        }

        public static Connection Create(ISecurityService securityService, IContainer container, string rCommandLineArguments, bool isUserCreated, bool fetchHostLoad) {
            var path = $"https://localhost:{container.HostPorts.First()}";
            var brokerConnectionInfo = BrokerConnectionInfo.Create(securityService, container.Name, path, rCommandLineArguments, fetchHostLoad);
            return new Connection(brokerConnectionInfo, path, isUserCreated, true);
        }

        private Connection(BrokerConnectionInfo brokerConnectionInfo, IConnectionInfo connectionInfo) 
            : base(connectionInfo) {
            BrokerConnectionInfo = brokerConnectionInfo;
        }

        private Connection(BrokerConnectionInfo brokerConnectionInfo, string path, bool isUserCreated, bool isDocker) 
            : base(brokerConnectionInfo.Name, path, brokerConnectionInfo.RCommandLineArguments, isUserCreated) {
            BrokerConnectionInfo = brokerConnectionInfo;
            IsDocker = isDocker;
        }

        public BrokerConnectionInfo BrokerConnectionInfo { get; }
        public Uri Uri => BrokerConnectionInfo.Uri;
        public bool IsRemote => BrokerConnectionInfo.IsUrlBased && !Uri.IsLoopback;
        public bool IsDocker { get; }
    }
}