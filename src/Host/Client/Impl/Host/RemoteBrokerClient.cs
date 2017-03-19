// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Net;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Host.Client.BrokerServices;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Client.Host {
    internal sealed class RemoteBrokerClient : BrokerClient {
        private readonly IConsole _console;
        private readonly ICoreShell _coreShell;
        private readonly object _verificationLock = new object();
        private readonly CancellationToken _cancellationToken;

        private string _certificateHash;
        private bool? _certificateValidationResult;

        static RemoteBrokerClient() {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        public RemoteBrokerClient(string name, BrokerConnectionInfo connectionInfo, ICoreShell coreShell, IConsole console, CancellationToken cancellationToken)
            : base(name, connectionInfo, new RemoteCredentialsDecorator(connectionInfo.CredentialAuthority, connectionInfo.Name, coreShell), coreShell.Log(), console) {
            _console = console;
            _coreShell = coreShell;
            _cancellationToken = cancellationToken;

            CreateHttpClient(connectionInfo.Uri);
            HttpClientHandler.ServerCertificateValidationCallback = ValidateCertificateHttpHandler;
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
            if (!url.StartsWithIgnoreCase("http://")) {
                _console.WriteError(string.Format(Resources.Error_RemoteUriNotSupported, url));
                return null;
            }

            return await WebServer.CreateWebServerAsync(url, HttpClient.BaseAddress.ToString(), Name, _coreShell, _console, cancellationToken);
        }

        protected override async Task<Exception> HandleHttpRequestExceptionAsync(HttpRequestException exception) {
            // Broker is not responding. Try regular ping.
            string status = await ConnectionInfo.Uri.GetMachineOnlineStatusAsync();
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
                if (_certificateValidationResult.HasValue) {
                    return _certificateValidationResult.Value;
                }

                var hashString = certificate.GetCertHashString();
                if (_certificateHash == null || !_certificateHash.EqualsOrdinal(hashString)) {
                    Log.Write(LogVerbosity.Minimal, MessageCategory.Warning, Resources.Trace_UntrustedCertificate.FormatInvariant(certificate.Subject));

                    var message = Resources.CertificateSecurityWarning.FormatInvariant(ConnectionInfo.Uri.Host);
                    _certificateValidationResult = _coreShell.Security().ValidateX509Certificate(certificate, message);
                    if (_certificateValidationResult.Value) {
                        _certificateHash = hashString;
                    }
                }
                return _certificateValidationResult.HasValue ? _certificateValidationResult.Value : false;
            }
        }
    }
}
