// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebSockets.Client;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Host.Client.BrokerServices;
using Microsoft.R.Host.Protocol;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Client.Host {
    internal abstract class BrokerClient : IBrokerClient, ICredentialsProvider {
        private static readonly TimeSpan HeartbeatTimeout =
#if DEBUG
            // In debug mode, increase the timeout significantly, so that when the host is paused in debugger,
            // the client won't immediately timeout and disconnect.
            TimeSpan.FromMinutes(10);
#else
            TimeSpan.FromSeconds(5);
#endif

        private readonly string _interpreterId;
        private AboutHost _aboutHost;
        private IRCallbacks _callbacks;
        private IntPtr _applicationWindowHandle;
        private int _display;

        protected DisposableBag DisposableBag { get; } = DisposableBag.Create<BrokerClient>();
        protected IActionLog Log { get; }
        protected WebRequestHandler HttpClientHandler { get; private set; }
        protected HttpClient HttpClient { get; private set; }

        public string Name { get; }
        public Uri Uri { get; }
        public bool IsRemote => !Uri.IsFile;
        public AboutHost AboutHost => _aboutHost ?? AboutHost.Empty;

        protected BrokerClient(string name, Uri brokerUri, string interpreterId, IActionLog log, IntPtr applicationWindowHandle) {
            Name = name;
            Uri = brokerUri;
            _interpreterId = interpreterId;
            Log = log;
            _applicationWindowHandle = applicationWindowHandle;
        }

        protected void CreateHttpClient(Uri baseAddress, ICredentials credentials) {

            HttpClientHandler = new WebRequestHandler() {
                PreAuthenticate = true,
                Credentials = credentials
            };

            HttpClientHandler.ServerCertificateValidationCallback += ValidateCertificate;

            HttpClient = new HttpClient(HttpClientHandler) {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(30),
            };

            HttpClient.DefaultRequestHeaders.Accept.Clear();
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0) {
                Log.WriteAsync(LogVerbosity.Minimal, MessageCategory.Error, Resources.Error_NoBrokerCertificate);
                _callbacks.WriteConsoleEx(Resources.Error_NoBrokerCertificate, OutputType.Error, CancellationToken.None).DoNotWait();
                return false;
            } else if (sslPolicyErrors != SslPolicyErrors.None) {
                if (Interlocked.CompareExchange(ref _display, 1, 0) == 0) {
                    var certificate2 = certificate as X509Certificate2;
                    Debug.Assert(certificate2 != null);
                    X509Certificate2UI.DisplayCertificate(certificate2, _applicationWindowHandle);
                    Interlocked.Exchange(ref _display, 0);
                }
                certificate.Reset();
            }
            return true;
        }

        public void Dispose() {
            DisposableBag.TryDispose();
        }

        /// <summary>
        /// Called before issuing an authenticated HTTP request. Implementation can refresh <see cref="HttpClientHandler.Credentials"/> if necessary.
        /// </summary>
        /// <remarks>
        /// For every call to this method, there will be a follow-up call to either <see cref="OnAuthenticationSucceeded"/> 
        /// or to <see cref="OnAuthenticationFailed"/> to indicate the result of authentication.
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        /// Retrieval of credentials was canceled by the user (for example, by clicking the "Cancel" button in the dialog).
        /// Usually, this indicates that the operation that asked for credentials should be canceled as well.
        /// </exception>
        protected abstract void UpdateCredentials();

        /// <summary>
        /// Called after the request that used credentials updated by an earlier call to <see cref="UpdateCredentials"/> completes.
        /// </summary>
        /// <param name="isValid">Whether the credentials were accepted or rejected by the server.</param>
        protected abstract void OnCredentialsValidated(bool isValid);

        public async Task PingAsync() {
            if (HttpClient != null) {
                // Just in case ping was disable for security reasons, try connecting to the broker anyway.
                try {
                    (await HttpClient.PostAsync("/ping", new StringContent(""))).EnsureSuccessStatusCode();
                } catch (HttpRequestException ex) {
                    throw await HandleHttpRequestExceptionAsync(ex);
                }
            }
        }

        public virtual async Task<RHost> ConnectAsync(string name, IRCallbacks callbacks, string rCommandLineArguments = null, int timeout = 3000,
            CancellationToken cancellationToken = default(CancellationToken), ReentrancyToken reentrancyToken = default(ReentrancyToken)) {

            DisposableBag.ThrowIfDisposed();
            _callbacks = callbacks;

            await TaskUtilities.SwitchToBackgroundThread();

            try {
                bool sessionExists = await IsSessionRunningAsync(name);

                WebSocket webSocket;
                while (true) {
                    if (!sessionExists) {
                        await CreateBrokerSessionAsync(name, rCommandLineArguments, cancellationToken);
                    }

                    try {
                        webSocket = await ConnectToBrokerAsync(name, cancellationToken);
                        break;
                    } catch (RHostDisconnectedException ex) when (
                        sessionExists && ((ex.InnerException as WebException)?.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound
                    ) {
                        // If we believed the session to be running, but failed to connect to its pipe, it probably terminated
                        // between our check and our attempt to connect. Retry, but recreate the session this time.
                        sessionExists = false;
                        continue;
                    }
                }

                var host = CreateRHost(name, callbacks, webSocket);
                await GetHostInformationAsync(cancellationToken);
                return host;
            } catch (HttpRequestException ex) {
                throw await HandleHttpRequestExceptionAsync(ex);
            }
        }

        public Task TerminateSessionAsync(string name, CancellationToken cancellationToken = default(CancellationToken)) {
            var sessionsService = new SessionsWebService(HttpClient, this);
            return sessionsService.DeleteAsync(name, cancellationToken);
        }

        protected virtual Task<Exception> HandleHttpRequestExceptionAsync(HttpRequestException exception) 
            => Task.FromResult<Exception>(new RHostDisconnectedException(Resources.Error_HostNotResponding.FormatInvariant(exception.Message), exception));

        private async Task<bool> IsSessionRunningAsync(string name) {
            var sessionsService = new SessionsWebService(HttpClient, this);
            var sessions = await sessionsService.GetAsync();
            return sessions.Any(s => s.Id == name);
        }

        private async Task CreateBrokerSessionAsync(string name, string rCommandLineArguments, CancellationToken cancellationToken) {
            rCommandLineArguments = rCommandLineArguments ?? string.Empty;
            var sessions = new SessionsWebService(HttpClient, this);
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
                ConfigureRequest = request => {
                    UpdateCredentials();
                    request.AuthenticationLevel = AuthenticationLevel.MutualAuthRequested;
                    request.Credentials = HttpClientHandler.Credentials;
                },
                InspectResponse = response => {
                    if (response.StatusCode == HttpStatusCode.Forbidden) {
                        throw new UnauthorizedAccessException();
                    }
                }
            };

            var pipeUri = new UriBuilder(HttpClient.BaseAddress) {
                Scheme = "ws",
                Path = $"sessions/{name}/pipe"
            }.Uri;

            WebSocket socket;
            while (true) {
                bool? isValidCredentials = null;
                try {
                    socket = await wsClient.ConnectAsync(pipeUri, cancellationToken);
                    isValidCredentials = true;
                    break;
                } catch (UnauthorizedAccessException) {
                    isValidCredentials = false;
                    continue;
                } catch (Exception ex) when (ex is InvalidOperationException || ex is WebException || ex is ProtocolViolationException) {
                    throw new RHostDisconnectedException(Resources.HttpErrorCreatingSession.FormatInvariant(ex.Message), ex);
                } finally {
                    if (isValidCredentials != null) {
                        OnCredentialsValidated(isValidCredentials.Value);
                    }
                }
            }
            return socket;
        }

        private RHost CreateRHost(string name, IRCallbacks callbacks, WebSocket socket) {
            var transport = new WebSocketMessageTransport(socket);

            var cts = new CancellationTokenSource();
            cts.Token.Register(() => { Log.RHostProcessExited(); });

            return new RHost(name, callbacks, transport, Log, cts);
        }

        private async Task GetHostInformationAsync(CancellationToken cancellationToken) {
            if (_aboutHost == null) {
                var response = await HttpClient.GetAsync("/about", cancellationToken);
                var s = await response.Content.ReadAsStringAsync();
                _aboutHost = !string.IsNullOrEmpty(s) ? JsonConvert.DeserializeObject<AboutHost>(s) : AboutHost.Empty;
            }
        }

        public virtual string HandleUrl(string url, CancellationToken ct) {
            return url;
        }

        void ICredentialsProvider.UpdateCredentials() => UpdateCredentials();

        void ICredentialsProvider.OnCredentialsValidated(bool isValid) => OnCredentialsValidated(isValid);
    }
}
