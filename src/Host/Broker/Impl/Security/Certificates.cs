// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Common.Core;
using static System.FormattableString;

namespace Microsoft.R.Host.Broker.Security {
    internal static class Certificates {
        public static X509Certificate2 GetCertificateForEncryption(SecurityOptions securityOptions) {
            if (string.IsNullOrWhiteSpace(securityOptions.X509CertificateFile)) {
                var certName = securityOptions.X509CertificateName ?? Invariant($"CN={Environment.MachineName}");
                return FindCertificate(certName);
            } else {
                if (securityOptions.X509CertificatePassword != null) {
                    return new X509Certificate2(securityOptions.X509CertificateFile, securityOptions.X509CertificatePassword);
                } else {
                    return new X509Certificate2(securityOptions.X509CertificateFile);
                }
            }
        }

        private static X509Certificate2 FindCertificate(string name) {
            StoreName[] stores;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                stores = new[] { StoreName.Root, StoreName.AuthRoot, StoreName.CertificateAuthority, StoreName.My };
            } else {
                stores = new[] { StoreName.Root, StoreName.CertificateAuthority };
            }

            foreach (StoreName storeName in stores) {
                using (var store = new X509Store(storeName, StoreLocation.LocalMachine)) {
                    try {
                        store.Open(OpenFlags.OpenExistingOnly);
                    } catch(CryptographicException) {
                        // Not all stores may be present
                        continue;
                    }

                    var collection = store.Certificates.Cast<X509Certificate2>();
                    var cert = collection.FirstOrDefault(c => c.FriendlyName.EqualsIgnoreCase(name));
                    if (cert == null) {
                        cert = collection.FirstOrDefault(c => c.Subject.EqualsIgnoreCase(name));
                        if (cert != null) {
                            return cert;
                        }
                    }
                }
            }
            return null;
        }
    }
}
