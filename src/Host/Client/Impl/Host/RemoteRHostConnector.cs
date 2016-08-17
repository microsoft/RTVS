// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.Host {
    internal sealed class RemoteRHostConnector : RHostConnector {
        public RemoteRHostConnector(Uri brokerUri)
            : base(brokerUri.Fragment) {

            CreateHttpClient();
            Broker.BaseAddress = brokerUri;
        }

        protected override HttpClientHandler GetHttpClientHandler() {
            return new HttpClientHandler {
                Credentials = new UICredentials()
            };
        }

        protected override void ConfigureWebSocketRequest(HttpWebRequest request) {
            request.AuthenticationLevel = AuthenticationLevel.MutualAuthRequested;
            request.Credentials = CredentialCache.DefaultNetworkCredentials;
        }

        protected override Task ConnectToBrokerAsync() {
            return Task.CompletedTask;
        }
    }
}
