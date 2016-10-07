// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.Security {
    internal sealed class TlsConfiguration {
        private readonly ILogger _logger;
        private readonly SecurityOptions _securityOptions;

        public TlsConfiguration(ILogger logger, SecurityOptions options) {
            _logger = logger;
            _securityOptions = options;
        }

        public HttpsConnectionFilterOptions GetHttpsOptions(IConfigurationRoot configuration) {
            var cert = GetCertificate(configuration);
            if (cert != null) {
                return new HttpsConnectionFilterOptions() {
                    ServerCertificate = cert,
                    ClientCertificateValidation = ClientCertificateValidationCallback,
                    ClientCertificateMode = ClientCertificateMode.NoCertificate,
                    SslProtocols = SslProtocols.Tls12
                };
            }
            return null;
        }

        private X509Certificate2 GetCertificate(IConfigurationRoot configuration) {
            if (IsLocalConnection(configuration)) {
                return null; // localhost, no TLS
            }

            X509Certificate2 certificate = null;
            var certName = _securityOptions.X509CertificateName ?? $"CN={Environment.MachineName}";
            certificate = Certificates.GetCertificateForEncryption(certName);
            if (certificate == null) {
                _logger.LogCritical(Resources.Critical_NoTlsCertificate, certName);
                Environment.Exit((int)BrokerExitCodes.NoCertificate);
            }

            _logger.LogInformation(Resources.Trace_CertificateIssuer, certificate.Issuer);
            _logger.LogInformation(Resources.Trace_CertificateSubject, certificate.Subject);

            return certificate;
        }

        private bool IsLocalConnection(IConfigurationRoot configuration) {
            try {
                Uri uri;
                var url = configuration.GetValue<string>("server.urls", null);
                if (Uri.TryCreate(url, UriKind.Absolute, out uri) && uri.IsLoopback) {
                    return true;
                }
            } catch (Exception) { }
            return false;
        }

        private static bool ClientCertificateValidationCallback(X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            return sslPolicyErrors == SslPolicyErrors.None;
        }
    }
}
