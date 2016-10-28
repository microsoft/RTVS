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
        private string  _certificateHash;

        static RemoteBrokerClient() {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        public RemoteBrokerClient(string name, Uri brokerUri, ICoreServices services, IConsole console)
            : base(name, brokerUri, brokerUri.Fragment, new RemoteCredentialsDecorator(brokerUri, services.Security), services.Log, console) {
            _console = console;
            _services = services;

            CreateHttpClient(brokerUri);
            HttpClientHandler.ServerCertificateValidationCallback = ValidateCertificateHttpHandler;
        }

        public override Task<string> HandleUrlAsync(string url, CancellationToken cancellationToken) {
            return WebServer.CreateWebServerAsync(url, HttpClient.BaseAddress.ToString(), cancellationToken);
        }

        protected override async Task<Exception> HandleHttpRequestExceptionAsync(HttpRequestException exception) {
            // Broker is not responsing. Try regular ping.
            string status = await Uri.GetMachineOnlineStatusAsync();
            return string.IsNullOrEmpty(status)
                ? new RHostDisconnectedException(Resources.Error_BrokerNotRunning, exception)
                : await base.HandleHttpRequestExceptionAsync(exception);
        }
        
        private bool ValidateCertificateHttpHandler(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            IsVerified = sslPolicyErrors == SslPolicyErrors.None;
            if (IsVerified) {
                return true;
            }

            if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable)) {
                Log.WriteAsync(LogVerbosity.Minimal, MessageCategory.Error, Resources.Error_NoBrokerCertificate).DoNotWait();
                _console.Write(Resources.Error_NoBrokerCertificate);
            } else {
                var hashString = certificate.GetCertHashString();
                if (_certificateHash == null || !_certificateHash.EqualsOrdinal(hashString)) {
                    Log.WriteAsync(LogVerbosity.Minimal, MessageCategory.Warning, Resources.Trace_UntrustedCertificate.FormatInvariant(certificate.Subject)).DoNotWait();

                    var message = Resources.CertificateSecurityWarning.FormatInvariant(Uri.Host);
                    var certificateTask = _services.Security.ValidateX509CertificateAsync(certificate, message);
                    _services.Tasks.Wait(certificateTask);

                    var accepted = certificateTask.Result;
                    if (accepted) {
                        _certificateHash = hashString;
                    }
                    return accepted;
                }
            }

            return IsVerified;
        }
    }
}
