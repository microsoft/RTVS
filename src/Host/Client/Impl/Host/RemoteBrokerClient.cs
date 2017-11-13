// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Client.Host {
    public sealed class RemoteBrokerClient : BrokerClient {
        private readonly IConsole _console;
        private readonly IServiceContainer _services;
        private readonly object _verificationLock = new object();
        private readonly CancellationToken _cancellationToken;
        private readonly IRSessionProvider _sessionProvider;

        private string _certificateHash;
        private bool? _certificateValidationResult;

        static RemoteBrokerClient() {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        public RemoteBrokerClient(string name, IRSessionProvider sessionProvider, BrokerConnectionInfo connectionInfo, IServiceContainer services, IConsole console, CancellationToken cancellationToken)
            : base(name, connectionInfo, new RemoteCredentialsDecorator(connectionInfo.CredentialAuthority, connectionInfo.Name, services), console, services) {
            _console = console;
            _services = services;
            _cancellationToken = cancellationToken;
            _sessionProvider = sessionProvider;

            CreateHttpClient(connectionInfo.Uri);
            HttpClientHandler.ServerCertificateCustomValidationCallback = ValidateCertificateHttpHandler;
        }

        public override async Task<RHost> ConnectAsync(HostConnectionInfo connectionInfo, CancellationToken cancellationToken = new CancellationToken()) {
            var host = await base.ConnectAsync(connectionInfo, cancellationToken);

            var aboutHost = await GetHostInformationAsync<AboutHost>(cancellationToken);
            var brokerIncompatibleMessage = aboutHost?.IsHostVersionCompatible();
            if (brokerIncompatibleMessage != null) {
                throw new RHostDisconnectedException(brokerIncompatibleMessage);
            }

            return host;
        }

        public override async Task<string> HandleUrlAsync(string url, CancellationToken cancellationToken) {
            var remotingService = _services.GetService<IRemotingWebServer>();
            Uri uri = new Uri(url);
            if (url.StartsWithIgnoreCase("http://")) {
                return await remotingService.HandleRemoteWebUrlAsync(url, HttpClient.BaseAddress.ToString(), Name, _console, cancellationToken);
            } else if (uri.AbsoluteUri.StartsWithIgnoreCase("file://") || url.StartsWithIgnoreCase("/")) {
                var fs = _services.GetService<IFileSystem>();
                return await remotingService.HandleRemoteStaticFileUrlAsync(url, _sessionProvider, _console, cancellationToken);
            } else if (!url.StartsWithIgnoreCase("http://") && _sessionProvider != null) {
                var fullpath = url;
                try {
                    var session = _sessionProvider.GetOrCreate("REPL");
                    if (!url.StartsWithIgnoreCase("http://") && await session.FileExistsAsync(url, cancellationToken)) {
                        fullpath = $"file:///{await session.NormalizePathAsync(url, cancellationToken)}";
                    }
                } catch (Exception ex) when (!ex.IsCriticalException()) {
                    // This is best effort to find the resource
                }
                var fs = _services.GetService<IFileSystem>();
                return await remotingService.HandleRemoteStaticFileUrlAsync(fullpath, _sessionProvider, _console, cancellationToken);
            } else {
                _console.WriteError(string.Format(Resources.Error_RemoteUriNotSupported, url));
                return null;
            }
        }

        protected override async Task<Exception> HandleHttpRequestExceptionAsync(HttpRequestException exception) {
            // Broker is not responding. Try regular ping.
            var status = await ConnectionInfo.Uri.GetMachineOnlineStatusAsync();
            return string.IsNullOrEmpty(status)
                ? new RHostDisconnectedException(Resources.Error_BrokerNotRunning.FormatInvariant(Name), exception)
                : new RHostDisconnectedException(Resources.Error_HostNotRespondingToPing.FormatInvariant(Name, exception.Message), exception);
        }

        private bool ValidateCertificateHttpHandler(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            if (_cancellationToken.IsCancellationRequested) {
                return false;
            }

            IsVerified = sslPolicyErrors == SslPolicyErrors.None;
            if (IsVerified) {
                return true;
            }

            if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable)) {
                Log.WriteLine(LogVerbosity.Minimal, MessageCategory.Error, Resources.Error_NoBrokerCertificate);
                _console.WriteError(Resources.Error_NoBrokerCertificate.FormatInvariant(Name));
                return false;
            }

            lock (_verificationLock) {
                if (ConnectionInfo.IsUrlBased && ConnectionInfo.Uri.IsLoopback) {
                    return true;
                }

                if (_certificateValidationResult.HasValue) {
                    return _certificateValidationResult.Value;
                }

                Log.Write(LogVerbosity.Minimal, MessageCategory.General, Resources.Trace_SSLPolicyErrors.FormatInvariant(sslPolicyErrors));
                var hashString = GetCertHashString(certificate.GetCertHash());
                if (_certificateHash == null || !_certificateHash.EqualsOrdinal(hashString)) {
                    Log.Write(LogVerbosity.Minimal, MessageCategory.Warning, Resources.Trace_UntrustedCertificate.FormatInvariant(certificate.Subject));

                    var message = Resources.CertificateSecurityWarning.FormatInvariant(ConnectionInfo.Uri.Host);
                    _certificateValidationResult = _services.Security().ValidateX509Certificate(certificate, message);
                    if (_certificateValidationResult.Value) {
                        _certificateHash = hashString;
                    }
                }
                return _certificateValidationResult ?? false;
            }
        }

        private string GetCertHashString(byte[] hash) {
            var sb = new StringBuilder(); 
            foreach (var t in hash) {
                sb.Append(t.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
