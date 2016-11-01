// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebSockets.Client;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Net;
using Microsoft.R.Host.Client.BrokerServices;
using Microsoft.R.Host.Protocol;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Client.Host {
    internal abstract class BrokerClient : IBrokerClient {
        private static readonly TimeSpan HeartbeatTimeout =
#if DEBUG
            // In debug mode, increase the timeout significantly, so that when the host is paused in debugger,
            // the client won't immediately timeout and disconnect.
            TimeSpan.FromMinutes(10);
#else
            TimeSpan.FromSeconds(5);
#endif

        private readonly string _interpreterId;
        private readonly ICredentialsDecorator _credentials;
        private readonly IConsole _console;

        protected DisposableBag DisposableBag { get; } = DisposableBag.Create<BrokerClient>();
        protected IActionLog Log { get; }
        protected WebRequestHandler HttpClientHandler { get; private set; }
        protected HttpClient HttpClient { get; private set; }

        public string Name { get; }
        public Uri Uri { get; }
        public bool IsRemote => !Uri.IsFile;
        public bool IsVerified { get; protected set; }

        protected BrokerClient(string name, Uri brokerUri, string interpreterId, ICredentialsDecorator credentials, IActionLog log, IConsole console) {
            Name = name;
            Uri = brokerUri;
            Log = log;

            _interpreterId = interpreterId;
            _credentials = credentials;
            _console = console;
        }

        protected void CreateHttpClient(Uri baseAddress) {
            HttpClientHandler = new WebRequestHandler {
                PreAuthenticate = true,
                Credentials = _credentials
            };

            HttpClient = new HttpClient(HttpClientHandler) {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(30),
            };

            HttpClient.DefaultRequestHeaders.Accept.Clear();
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Dispose() => DisposableBag.TryDispose();

        public async Task PingAsync() {
            if (HttpClient != null) {
                // Just in case ping was disable for security reasons, try connecting to the broker anyway.
                try {
                    await GetHostInformationAsync(CancellationToken.None);
                } catch (HttpRequestException ex) {
                    throw await HandleHttpRequestExceptionAsync(ex);
                }
            }
        }

        public async Task<AboutHost> GetHostInformationAsync(CancellationToken cancellationToken) {
            string result = null;
            try {
                var response = await HttpClient.GetAsync("/about", cancellationToken);
                result = await response.Content.ReadAsStringAsync();
            } catch (OperationCanceledException) { } catch(HttpRequestException) { } 
            return !string.IsNullOrEmpty(result) ? JsonConvert.DeserializeObject<AboutHost>(result) : AboutHost.Empty;
        }

        public virtual async Task<RHost> ConnectAsync(BrokerConnectionInfo connectionInfo, CancellationToken cancellationToken = default(CancellationToken)) {
            DisposableBag.ThrowIfDisposed();
            await TaskUtilities.SwitchToBackgroundThread();

            try {
                var sessionExists = connectionInfo.PreserveSessionData && await IsSessionRunningAsync(connectionInfo.Name, cancellationToken);
                if (sessionExists) {
                    var terminateRDataSave = await _console.PromptYesNoAsync(Resources.AbortRDataAutosave);
                    if (!terminateRDataSave) {
                        while (await IsSessionRunningAsync(connectionInfo.Name, cancellationToken)) {
                            await Task.Delay(500, cancellationToken);
                        }
                    }
                }

                await CreateBrokerSessionAsync(connectionInfo.Name, connectionInfo.RCommandLineArguments, cancellationToken);
                var webSocket = await ConnectToBrokerAsync(connectionInfo.Name, cancellationToken);
                var host = CreateRHost(connectionInfo.Name, connectionInfo.Callbacks, webSocket);
                await GetHostInformationAsync(cancellationToken);
                return host;
            } catch (HttpRequestException ex) {
                throw await HandleHttpRequestExceptionAsync(ex);
            }
        }

        public Task TerminateSessionAsync(string name, CancellationToken cancellationToken = default(CancellationToken)) {
            var sessionsService = new SessionsWebService(HttpClient, _credentials);
            return sessionsService.DeleteAsync(name, cancellationToken);
        }

        protected virtual Task<Exception> HandleHttpRequestExceptionAsync(HttpRequestException exception)
            => Task.FromResult<Exception>(new RHostDisconnectedException(Resources.Error_HostNotResponding.FormatInvariant(exception.Message), exception));

        private async Task<bool> IsSessionRunningAsync(string name, CancellationToken cancellationToken) {
            var sessionsService = new SessionsWebService(HttpClient, _credentials);
            var sessions = await sessionsService.GetAsync(cancellationToken);
            return sessions.Any(s => s.Id == name);
        }

        private async Task CreateBrokerSessionAsync(string name, string rCommandLineArguments, CancellationToken cancellationToken) {
            rCommandLineArguments = rCommandLineArguments ?? string.Empty;
            var sessions = new SessionsWebService(HttpClient, _credentials);
            try {
                await sessions.PutAsync(name, new SessionCreateRequest {
                    InterpreterId = _interpreterId,
                    CommandLineArguments = rCommandLineArguments,
                }, cancellationToken);
            } catch (BrokerApiErrorException apiex) {
                throw new RHostDisconnectedException(apiex);
            }
        }

        private async Task<WebSocket> ConnectToBrokerAsync(string name, CancellationToken cancellationToken) {
            var wsClient = new WebSocketClient {
                KeepAliveInterval = HeartbeatTimeout,
                SubProtocols = { "Microsoft.R.Host" },
                InspectResponse = response => {
                    if (response.StatusCode == HttpStatusCode.Forbidden) {
                        throw new UnauthorizedAccessException();
                    }
                }
            };

            var pipeUri = new UriBuilder(HttpClient.BaseAddress) {
                Scheme = HttpClient.BaseAddress.IsHttps() ? "wss" : "ws",
                Path = $"sessions/{name}/pipe"
            }.Uri;

            while (true) {
                var request = wsClient.CreateRequest(pipeUri);

                using (await _credentials.LockCredentialsAsync(cancellationToken)) {
                    try {
                        request.AuthenticationLevel = AuthenticationLevel.MutualAuthRequested;
                        request.Credentials = HttpClientHandler.Credentials;
                        return await wsClient.ConnectAsync(request, cancellationToken);
                    } catch (UnauthorizedAccessException) {
                        _credentials.InvalidateCredentials();
                    } catch (Exception ex) when (ex is InvalidOperationException) {
                        throw new RHostDisconnectedException(Resources.HttpErrorCreatingSession.FormatInvariant(ex.Message), ex);
                    }
                }
            }
        }

        private RHost CreateRHost(string name, IRCallbacks callbacks, WebSocket socket) {
            var transport = new WebSocketMessageTransport(socket);

            var cts = new CancellationTokenSource();
            cts.Token.Register(() => { Log.RHostProcessExited(); });

            return new RHost(name, callbacks, transport, Log, cts);
        }

        public virtual Task<string> HandleUrlAsync(string url, CancellationToken cancellationToken)  => Task.FromResult(url);
    }
}
