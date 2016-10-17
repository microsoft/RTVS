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
using Microsoft.R.Host.Client.BrokerServices;

namespace Microsoft.R.Host.Client.Host {
    internal sealed class RemoteBrokerClient : BrokerClient {
        private readonly IConsole _console;
        private readonly ICoreServices _services;

        static RemoteBrokerClient() {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateCertificateServicePoint);
        }

        public RemoteBrokerClient(string name, Uri brokerUri, IActionLog log, IConsole console, ISecurityService securityService)
            : base(name, brokerUri, brokerUri.Fragment, new RemoteCredentialsDecorator(brokerUri, securityService), log, console) {
            _console = console;
            _services = services;

            CreateHttpClient(brokerUri);
            HttpClientHandler.ServerCertificateValidationCallback = ValidateCertificateHttpHandler;
        }

        public override string HandleUrl(string url, CancellationToken ct) {
            return WebServer.CreateWebServer(url, HttpClient.BaseAddress.ToString(), ct);
        }

        protected override async Task<Exception> HandleHttpRequestExceptionAsync(HttpRequestException exception) {
            // Broker is not responsing. Try regular ping.
            string status = await Uri.GetMachineOnlineStatusAsync();
            return string.IsNullOrEmpty(status)
                ? new RHostDisconnectedException(Resources.Error_BrokerNotRunning, exception)
                : await base.HandleHttpRequestExceptionAsync(exception);
        }

        private static bool ValidateCertificateServicePoint(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

        private bool ValidateCertificateHttpHandler(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            IsVerified = sslPolicyErrors == SslPolicyErrors.None;
            if (!IsVerified) {
                if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable)) {
                    Log.WriteAsync(LogVerbosity.Minimal, MessageCategory.Error, Resources.Error_NoBrokerCertificate);
                    _console.Write(Resources.Error_NoBrokerCertificate);
                } else {
                    Log.WriteAsync(LogVerbosity.Minimal, MessageCategory.Warning, Resources.Trace_UntrustedCertificate.FormatInvariant(certificate.Subject)).DoNotWait();

                    var message = Resources.CertificateSecurityWarning.FormatInvariant(Uri.Host);
                    return _services.Security.ValidateX509CertificateAsync(certificate, message).GetAwaiter().GetResult();
                }
            }
            return IsVerified;
        }
    }
}
