// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.R.Host.Broker.Startup;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.Security {
    public sealed class TlsConfiguration : IConfigureOptions<KestrelServerOptions> {
        private readonly IApplicationLifetime _lifetime;
        private readonly ILogger<TlsConfiguration> _logger;
        private readonly StartupOptions _startupOptions;
        private readonly SecurityOptions _securityOptions;

        public TlsConfiguration(IApplicationLifetime lifetime, ILogger<TlsConfiguration> logger, IOptions<StartupOptions> startupOptions, IOptions<SecurityOptions> securityOptions) {
            _lifetime = lifetime;
            _logger = logger;
            _startupOptions = startupOptions.Value;
            _securityOptions = securityOptions.Value;
        }

        public void Configure(KestrelServerOptions options) {
            var httpsOptions = GetHttpsOptions();
            if (httpsOptions == null) {
                _logger.LogCritical(Resources.Critical_NoTlsCertificate, _securityOptions.X509CertificateName);
                _lifetime.StopApplication();

                if (!_startupOptions.IsService) {
                    Environment.Exit((int)BrokerExitCodes.NoCertificate);
                }
            }

            options.UseHttps(httpsOptions);
        }

        private HttpsConnectionFilterOptions GetHttpsOptions() {            
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
