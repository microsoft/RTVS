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
using static System.FormattableString;

namespace Microsoft.R.Host.Broker.Security {
    public sealed class TlsConfiguration {
        private readonly ILogger _logger;
        private readonly SecurityOptions _securityOptions;

        public TlsConfiguration(ILogger logger, SecurityOptions options) {
            _logger = logger;
            _securityOptions = options;
        }

        public HttpsConnectionFilterOptions GetHttpsOptions() {            
            var cert = GetCertificate();
            if (cert != null) {
                return new HttpsConnectionFilterOptions {
                    ServerCertificate = cert,
                    ClientCertificateValidation = ClientCertificateValidationCallback,
                    ClientCertificateMode = ClientCertificateMode.NoCertificate,
                    SslProtocols = SslProtocols.Tls12
                };
            }
            return null;
        }

        private X509Certificate2 GetCertificate() {
            X509Certificate2 certificate = Certificates.GetCertificateForEncryption(_securityOptions);
            if (certificate == null) {
                return null;
            }

            _logger.LogInformation(Resources.Trace_CertificateIssuer, certificate.Issuer);
            _logger.LogInformation(Resources.Trace_CertificateSubject, certificate.Subject);

            return certificate;
        }

        private static bool ClientCertificateValidationCallback(X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            return sslPolicyErrors == SslPolicyErrors.None;
        }
    }
}
