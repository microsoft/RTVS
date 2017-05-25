// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Json;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Net;
using Microsoft.Common.Core.Services;
using Microsoft.R.Host.Client.BrokerServices;
using Microsoft.R.Host.Protocol;
using static System.FormattableString;

namespace Microsoft.R.Host.Client.Host {
    public abstract class BrokerClient : IBrokerClient {
        private static readonly TimeSpan HeartbeatTimeout =
#if DEBUG
            // In debug mode, increase the timeout significantly, so that when the host is paused in debugger,
            // the client won't immediately timeout and disconnect.
            TimeSpan.FromMinutes(10);
#else
            TimeSpan.FromSeconds(5);
#endif
        private static IReadOnlyDictionary<Type, string> _typeToEndpointMap = new Dictionary<Type, string>() {
            { typeof(AboutHost), "info/about"},
            { typeof(HostLoad), "info/load"}
        };

        private readonly string _interpreterId;
        private readonly string _rCommandLineArguments;
        private readonly ICredentialsDecorator _credentials;
        private readonly IConsole _console;
        private readonly IServiceContainer _services;

        protected DisposableBag DisposableBag { get; } = DisposableBag.Create<BrokerClient>();
        protected IActionLog Log { get; }
        protected WinHttpHandler HttpClientHandler { get; private set; }
        protected HttpClient HttpClient { get; private set; }

        public BrokerConnectionInfo ConnectionInfo { get; }
        public string Name { get; }
        public bool IsRemote => ConnectionInfo.IsRemote;
        public bool IsVerified { get; protected set; }

        protected BrokerClient(string name, BrokerConnectionInfo connectionInfo, ICredentialsDecorator credentials, IConsole console, IServiceContainer services) {
            Name = name;
            Log = services.Log();

            _rCommandLineArguments = connectionInfo.RCommandLineArguments;
            _interpreterId = connectionInfo.InterpreterId;
            _credentials = credentials;
            _console = console;
            ConnectionInfo = connectionInfo;
            _services = services;
        }

        protected void CreateHttpClient(Uri baseAddress) {
            HttpClientHandler = new WinHttpHandler {
                PreAuthenticate = true,
                ServerCredentials = _credentials
            };

            try {
                HttpClient = new HttpClient(HttpClientHandler) {
                    BaseAddress = baseAddress,
                    Timeout = TimeSpan.FromSeconds(30),
                };
            } catch(ArgumentException) {
                var message = Resources.Error_InvalidUrl.FormatInvariant(baseAddress);
                _console.WriteLine(message); // Output now since progress dialog may eat the exception
                throw new RHostDisconnectedException(message);
            }

            HttpClient.DefaultRequestHeaders.Accept.Clear();
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Dispose() => DisposableBag.TryDispose();

        public async Task<T> GetHostInformationAsync<T>(CancellationToken cancellationToken) {
            string result = null;
            try {
                string endpoint;
                if (!_typeToEndpointMap.TryGetValue(typeof(T), out endpoint)) {
                    throw new ArgumentException($"There is no endpoint for type {typeof(T)}");
                }

                if (HttpClient != null) {
                    var response = await HttpClient.GetAsync(endpoint, cancellationToken);
                    result = response != null ? await response.Content.ReadAsStringAsync() : null;
                }

                return !string.IsNullOrEmpty(result) ? Json.DeserializeObject<T>(result) : default(T);
            } catch (HttpRequestException ex) {
                throw new RHostDisconnectedException(Resources.Error_HostNotResponding.FormatInvariant(Name, ex.Message), ex);
            }
        }

        public async Task DeleteProfileAsync(CancellationToken cancellationToken) {
            await TaskUtilities.SwitchToBackgroundThread();
            try {
                var sessionsService = new ProfileWebService(HttpClient, _credentials, Log);
                await sessionsService.DeleteAsync(cancellationToken);
            } catch (HttpRequestException ex) {
                throw new RHostDisconnectedException(Resources.Error_HostNotResponding.FormatInvariant(Name, ex.Message), ex);
            }
        }

        public virtual async Task<RHost> ConnectAsync(HostConnectionInfo connectionInfo, CancellationToken cancellationToken = default(CancellationToken)) {
            DisposableBag.ThrowIfDisposed();
            await TaskUtilities.SwitchToBackgroundThread();

            var uniqueSessionName = $"{connectionInfo.Name}_{ConnectionInfo.ParametersId}";
            try {
                var sessionExists = connectionInfo.PreserveSessionData && await IsSessionRunningAsync(uniqueSessionName, cancellationToken);
                if (sessionExists) {
                    var terminateRDataSave = await _console.PromptYesNoAsync(Resources.AbortRDataAutosave, cancellationToken);
                    if (!terminateRDataSave) {
                        while (await IsSessionRunningAsync(uniqueSessionName, cancellationToken)) {
                            await Task.Delay(500, cancellationToken);
                        }
                    }
                }

                await CreateBrokerSessionAsync(uniqueSessionName, connectionInfo.UseRHostCommandLineArguments, connectionInfo.IsInteractive, cancellationToken);
                var webSocket = await ConnectToBrokerAsync(uniqueSessionName, cancellationToken);
                return CreateRHost(uniqueSessionName, connectionInfo.Callbacks, webSocket);
            } catch (HttpRequestException ex) {
                throw await HandleHttpRequestExceptionAsync(ex);
            }
        }

        public Task TerminateSessionAsync(string name, CancellationToken cancellationToken = default(CancellationToken)) {
            var sessionsService = new SessionsWebService(HttpClient, _credentials, Log);
            return sessionsService.DeleteAsync(name, cancellationToken);
        }

        protected virtual Task<Exception> HandleHttpRequestExceptionAsync(HttpRequestException exception)
            => Task.FromResult<Exception>(new RHostDisconnectedException(Resources.Error_HostNotResponding.FormatInvariant(Name, exception.Message), exception));

        private async Task<bool> IsSessionRunningAsync(string name, CancellationToken cancellationToken) {
            var sessionsService = new SessionsWebService(HttpClient, _credentials, Log);
            var sessions = await sessionsService.GetAsync(cancellationToken);
            return sessions.Any(s => s.Id == name);
        }

        private async Task CreateBrokerSessionAsync(string name, bool useRCommandLineArguments, bool isInteractive, CancellationToken cancellationToken) {
            var rCommandLineArguments = useRCommandLineArguments && _rCommandLineArguments != null ? _rCommandLineArguments : null;
            var sessions = new SessionsWebService(HttpClient, _credentials, Log);
            using (Log.Measure(LogVerbosity.Normal, Invariant($"Create broker session \"{name}\""))) {
                try {
                    await sessions.PutAsync(name, new SessionCreateRequest {
                        InterpreterId = _interpreterId,
                        CommandLineArguments = rCommandLineArguments,
                        IsInteractive = isInteractive,
                    }, cancellationToken);
                } catch (BrokerApiErrorException apiex) {
                    throw new RHostDisconnectedException(MessageFromBrokerApiException(apiex), apiex);
                }
            }
        }

        private async Task<WebSocket> ConnectToBrokerAsync(string name, CancellationToken cancellationToken) {
            using (Log.Measure(LogVerbosity.Normal, Invariant($"Connect to broker session \"{name}\""))) {
                var wsClientFactory = _services.GetService<IWebSocketClientService>();
                var wsClient = wsClientFactory.Create(new List<string> { "Microsoft.R.Host" });
                wsClient.KeepAliveInterval = HeartbeatTimeout;
                wsClient.InspectResponse = response => {
                    if (response.StatusCode == HttpStatusCode.Forbidden) {
                        throw new UnauthorizedAccessException();
                    }
                };

                var pipeUri = new UriBuilder(HttpClient.BaseAddress) {
                    Scheme = HttpClient.BaseAddress.IsHttps() ? "wss" : "ws",
                    Path = $"sessions/{name}/pipe"
                }.Uri;

                while (true) {
                    var request = wsClient.CreateRequest(pipeUri, HttpClientHandler.ServerCredentials);

                    using (await _credentials.LockCredentialsAsync(cancellationToken)) {
                        try {
                            return await wsClient.ConnectAsync(request, cancellationToken);
                        } catch (UnauthorizedAccessException) {
                            _credentials.InvalidateCredentials();
                        } catch (Exception ex) when (ex is InvalidOperationException) {
                            throw new RHostDisconnectedException(Resources.HttpErrorCreatingSession.FormatInvariant(Name, ex.Message), ex);
                        }
                    }
                }
            }
        }

        private RHost CreateRHost(string name, IRCallbacks callbacks, WebSocket socket) {
            var transport = new WebSocketMessageTransport(socket);
            return new RHost(name, callbacks, transport, Log);
        }

        private string MessageFromBrokerApiException(BrokerApiErrorException ex) {
            switch (ex.ApiError) {
                case BrokerApiError.NoRInterpreters:
                    return Resources.Error_NoRInterpreters;
                case BrokerApiError.InterpreterNotFound:
                    return Resources.Error_InterpreterNotFound.FormatInvariant(_interpreterId);
                case BrokerApiError.UnableToStartRHost:
                    if (!string.IsNullOrEmpty(ex.Message)) {
                        return Resources.Error_UnableToStartHostException.FormatInvariant(Name, ex.Message);
                    }
                    return Resources.Error_UnknownError;
                case BrokerApiError.PipeAlreadyConnected:
                    return Resources.Error_PipeAlreadyConnected;
                case BrokerApiError.Win32Error:
                    if (!string.IsNullOrEmpty(ex.Message)) {
                        return Resources.Error_BrokerWin32Error.FormatInvariant(ex.Message);
                    }
                    return Resources.Error_BrokerUnknownWin32Error;
            }

            Debug.Fail("No localized resources for broker API error" + ex.ApiError.ToString());
            return ex.ApiError.ToString();
        }

        public virtual Task<string> HandleUrlAsync(string url, CancellationToken cancellationToken)  => Task.FromResult(url);
    }
}
