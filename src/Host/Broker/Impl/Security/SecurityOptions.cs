// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Broker.Security {
    public class SecurityOptions {
        public string Secret { get; set; }

        /// <summary>
        /// Local group permitted to connect
        /// </summary>
        public string AllowedGroup { get; set; } = "Users";

        /// <summary>
        /// Friendly name of the certificate installed for the TLS (SSL)
        /// </summary>
        public string X509CertificateName { get; set; }

        /// <summary>
        /// Path to the certificate for use with TLS (SSL)
        /// </summary>
        public string X509CertificateFile { get; set; }

        /// <summary>
        /// Password to access private key in the certificate <see cref="X509CertificateFile"/>
        /// </summary>
        public string X509CertificatePassword { get; set; }
    }
}
