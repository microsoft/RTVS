// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebSockets.Client;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Logging;
using Microsoft.R.Host.BrokerServices;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Client.Host {
    internal abstract class RHostConnector : IRHostConnector {
        private static readonly TimeSpan HeartbeatTimeout =
#if DEBUG
            // In debug mode, increase the timeout significantly, so that when the host is paused in debugger,
            // the client won't immediately timeout and disconnect.
            TimeSpan.FromMinutes(10);
#else
            TimeSpan.FromSeconds(5);
#endif

        protected DisposableBag DisposableBag { get; } = DisposableBag.Create<RHostConnector>();

        private readonly LinesLog _log;
        private HttpClient _broker;
        private string _interpreterId;

        protected HttpClient Broker => _broker;

        protected RHostConnector(string interpreterId) {
            _interpreterId = interpreterId;
            _log = new LinesLog(FileLogWriter.InTempFolder("Microsoft.R.Host.BrokerConnector"));
        }

        protected void CreateHttpClient() {
            _broker = new HttpClient(GetHttpClientHandler()) {
                Timeout = TimeSpan.FromSeconds(30)
            };

            _broker.DefaultRequestHeaders.Accept.Clear();
            _broker.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected abstract HttpClientHandler GetHttpClientHandler();

        protected abstract void ConfigureWebSocketRequest(HttpWebRequest request);

        public virtual void Dispose() {
            DisposableBag.TryMarkDisposed();
        }

        protected abstract Task ConnectToBrokerAsync();

        public async Task PingAsync() {
            (await _broker.PostAsync("/ping", new StringContent(""))).EnsureSuccessStatusCode();
        }

        private async Task PingWorker() {
            try {
                while (true) {
                    await PingAsync();
                    await Task.Delay(1000);
                }
            } catch (OperationCanceledException) {
            } catch (HttpRequestException) {
            }
        }

        public async Task<RHost> Connect(string name, IRCallbacks callbacks, string rCommandLineArguments = null, int timeout = 3000, CancellationToken cancellationToken = new CancellationToken()) {
            DisposableBag.ThrowIfDisposed();
            await TaskUtilities.SwitchToBackgroundThread();

            await ConnectToBrokerAsync();

            rCommandLineArguments = rCommandLineArguments ?? string.Empty;

            try {
                var sessions = new SessionsWebService(_broker);
                await sessions.PutAsync(name, new SessionCreateRequest {
                    InterpreterId = _interpreterId,
                    CommandLineArguments = rCommandLineArguments,
                });
            } catch (HttpRequestException ex) {
                // If UICredentials was canceled by the user, we'll get HttpRequestException(WebException(OperationCanceledException)) here.
                // Unwrap all the extraneous layers, and rethrow the cancellation.
                var oce = ex?.InnerException?.InnerException as OperationCanceledException;
                if (oce != null) {
                    throw oce;
                }

                throw new RHostDisconnectedException("HTTP error while creating session: " + ex.Message, ex);
            }

            var wsClient = new WebSocketClient {
                KeepAliveInterval = HeartbeatTimeout,
                SubProtocols = { "Microsoft.R.Host" },
                ConfigureRequest = ConfigureWebSocketRequest
            };

            var pipeUri = new UriBuilder(_broker.BaseAddress) {
                Scheme = "ws",
                Path = $"sessions/{name}/pipe"
            }.Uri;
            var socket = await wsClient.ConnectAsync(pipeUri, cancellationToken);

            var transport = new WebSocketMessageTransport(socket);

            var cts = new CancellationTokenSource();
            cts.Token.Register(() => {
                _log.RHostProcessExited();
            });

            var host = new RHost(name, callbacks, transport, null, cts);
            return host;
        }

    }
}
