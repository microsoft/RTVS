// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Microsoft.Common.Core.Security;
using Microsoft.R.Components.Containers;
using Microsoft.R.Containers;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.R.Components.ConnectionManager {
    public class Connection : ConnectionInfo, IConnection {
        public static Connection Create(ISecurityService securityService, IContainerManager containers, IConnectionInfo connectionInfo, bool fetchHostLoad) {
            var brokerConnectionInfo = BrokerConnectionInfo.Create(securityService, connectionInfo.Name, connectionInfo.Path, connectionInfo.RCommandLineArguments, fetchHostLoad);
            var uri = brokerConnectionInfo.Uri;
            var container = brokerConnectionInfo.IsUrlBased && uri.IsLoopback
                ? containers.GetContainers().FirstOrDefault(c => c.HostPorts.Contains(uri.Port))
                : null;
            return new Connection(brokerConnectionInfo, connectionInfo, container);
        }

        public static Connection Create(ISecurityService securityService, string name, string path, string rCommandLineArguments, bool isUserCreated, bool fetchHostLoad) {
            var brokerConnectionInfo = BrokerConnectionInfo.Create(securityService, name, path, rCommandLineArguments, fetchHostLoad);
            return new Connection(brokerConnectionInfo, path, isUserCreated, null);
        }

        public static Connection Create(ISecurityService securityService, IContainer container, string rCommandLineArguments, bool isUserCreated, bool fetchHostLoad) {
            var path = $"https://localhost:{container.HostPorts.First()}";
            var brokerConnectionInfo = BrokerConnectionInfo.Create(securityService, container.Name, path, rCommandLineArguments, fetchHostLoad);
            return new Connection(brokerConnectionInfo, path, isUserCreated, container);
        }

        private Connection(BrokerConnectionInfo brokerConnectionInfo, IConnectionInfo connectionInfo, IContainer container) 
            : base(connectionInfo) {
            BrokerConnectionInfo = brokerConnectionInfo;
            Container = container;
        }

        private Connection(BrokerConnectionInfo brokerConnectionInfo, string path, bool isUserCreated, IContainer container) 
            : base(brokerConnectionInfo.Name, path, brokerConnectionInfo.RCommandLineArguments, isUserCreated) {
            BrokerConnectionInfo = brokerConnectionInfo;
            Container = container;
        }

        public BrokerConnectionInfo BrokerConnectionInfo { get; }
        public IContainer Container { get; }

        public Uri Uri => BrokerConnectionInfo.Uri;
        public string ContainerName => Container?.Name;
        public bool IsRemote => BrokerConnectionInfo.IsUrlBased && !Uri.IsLoopback;
        public bool IsContainer => Container != null;
    }
}