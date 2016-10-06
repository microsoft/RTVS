// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Net;
using Microsoft.R.Host.Client.BrokerServices;

namespace Microsoft.R.Host.Client.Host {
    internal sealed class RemoteBrokerClient : BrokerClient {
        private readonly IntPtr _applicationWindowHandle;
        private readonly string _authority;
        private readonly ICredentialsDecorator _credentials;
        private int _certificateUIActive;

        static RemoteBrokerClient() {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public RemoteBrokerClient(string name, Uri brokerUri, IActionLog log, IntPtr applicationWindowHandle)
            : base(name, brokerUri, brokerUri.Fragment, log, applicationWindowHandle) {
            _applicationWindowHandle = applicationWindowHandle;

            _credentials = new RemoteCredentialsDecorator(applicationWindowHandle, brokerUri);
            _authority = new UriBuilder { Scheme = brokerUri.Scheme, Host = brokerUri.Host, Port = brokerUri.Port }.ToString();
            CreateHttpClient(brokerUri);
        }

        public override string HandleUrl(string url, CancellationToken ct) {
            return WebServer.CreateWebServer(url, HttpClient.BaseAddress.ToString(), ct);
        }

        protected override ICredentialsDecorator Credentials => _credentials;

        protected override async Task<Exception> HandleHttpRequestExceptionAsync(HttpRequestException exception) {
            // Broker is not responsing. Try regular ping.
            string status = await Uri.GetMachineOnlineStatusAsync();
            return string.IsNullOrEmpty(status)
                ? new RHostDisconnectedException(Resources.Error_BrokerNotRunning, exception)
                : await base.HandleHttpRequestExceptionAsync(exception);
        }

        protected override void CreateHttpClient(Uri baseAddress) {
            base.CreateHttpClient(baseAddress);
            HttpClientHandler.ServerCertificateValidationCallback += ValidateCertificate;
        }

        private bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            if (sslPolicyErrors == SslPolicyErrors.None) {
                return true;
            }

            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0) {
                Log.WriteAsync(LogVerbosity.Minimal, MessageCategory.Error, Resources.Error_NoBrokerCertificate);
                Callbacks.WriteConsoleEx(Resources.Error_NoBrokerCertificate, OutputType.Error, CancellationToken.None).DoNotWait();
            } else if (Interlocked.CompareExchange(ref _certificateUIActive, 1, 0) == 0) {
                Log.WriteAsync(LogVerbosity.Minimal, MessageCategory.Warning, Resources.Trace_UntrustedCertificate.FormatInvariant(certificate.Subject)).DoNotWait();

                var certificate2 = certificate as X509Certificate2;
                Debug.Assert(certificate2 != null);
                X509Certificate2UI.DisplayCertificate(certificate2, _applicationWindowHandle);
                Interlocked.Exchange(ref _certificateUIActive, 0);
                certificate.Reset();
            }

            return false;
        }
    }
}
