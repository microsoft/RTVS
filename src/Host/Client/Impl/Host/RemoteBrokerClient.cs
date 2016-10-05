// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Logging;
using Microsoft.R.Host.Client.BrokerServices;

namespace Microsoft.R.Host.Client.Host {
    internal sealed class RemoteBrokerClient : BrokerClient {
        public RemoteBrokerClient(string name, Uri brokerUri, IntPtr applicationWindowHandle, IActionLog log)
            : base(name, brokerUri, brokerUri.Fragment, new RemoteCredentialsDecorator(applicationWindowHandle, brokerUri), log) {

            CreateHttpClient(brokerUri);
        }

        public override string HandleUrl(string url, CancellationToken ct) {
            return WebServer.CreateWebServer(url, HttpClient.BaseAddress.ToString(), ct);
        }

        protected override async Task<Exception> HandleHttpRequestExceptionAsync(HttpRequestException exception) {
            // Broker is not responsing. Try regular ping.
            string status = await GetMachineOnlineStatusAsync();
            return string.IsNullOrEmpty(status)
                ? new RHostDisconnectedException(Resources.Error_BrokerNotRunning, exception)
                : await base.HandleHttpRequestExceptionAsync(exception);
        }

        private async Task<string> GetMachineOnlineStatusAsync() {
            if (Uri.IsFile) {
                return string.Empty;
            }

            try {
                var ping = new Ping();
                var reply = await ping.SendPingAsync(Uri.Host, 5000);
                if (reply.Status != IPStatus.Success) {
                    return reply.Status.ToString();
                }
            } catch (PingException pex) {
                var pingMessage = pex.InnerException?.Message ?? pex.Message;
                if (!string.IsNullOrEmpty(pingMessage)) {
                    return pingMessage;
                }
            } catch (SocketException sx) {
                return sx.Message;
            }
            return string.Empty;
        }

    }
}
