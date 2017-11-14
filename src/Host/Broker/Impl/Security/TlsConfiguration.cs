// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Common.Core;
using Microsoft.Extensions.Logging;

namespace Microsoft.R.Host.Broker.Security {
    public sealed class TlsConfiguration {
        private readonly ILogger<TlsConfiguration> _logger;
        private readonly SecurityOptions _securityOptions;

        public TlsConfiguration(ILogger<TlsConfiguration> logger, SecurityOptions securityOptions) {
            _logger = logger;
            _securityOptions = securityOptions;
        }

        public HttpsConnectionAdapterOptions GetHttpsOptions() {
            var cert = GetCertificate();
            if (cert != null) {
                return new HttpsConnectionAdapterOptions {
                    ServerCertificate = cert,
                    ClientCertificateValidation = ClientCertificateValidationCallback,
                    ClientCertificateMode = ClientCertificateMode.NoCertificate,
                    SslProtocols = SslProtocols.Tls12
                };
            }
            return null;
        }

        private X509Certificate2 GetCertificate() {
            X509Certificate2 certificate;
            try {
                certificate = Certificates.GetCertificateForEncryption(_securityOptions);
            } catch (Exception ex) {
                _logger.LogError(Resources.Error_UnableToGetCertificateForEncryption.FormatInvariant(ex.Message));
                return null;
            }

            if (certificate == null) {
                return null;
            }

            _logger.LogInformation(Resources.Trace_CertificateIssuer, certificate.Issuer);
            _logger.LogInformation(Resources.Trace_CertificateSubject, certificate.Subject);

            return certificate;
        }

        private static bool ClientCertificateValidationCallback(X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            => sslPolicyErrors == SslPolicyErrors.None;
    }
}
