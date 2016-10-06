// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Common.Core;

namespace Microsoft.R.Host.Broker.Security {
    internal static class Certificates {
        public static X509Certificate2 GetCertificateForEncryption(string certName) {
            return FindCertificate(certName);
        }

        private static X509Certificate2 FindCertificate(string name) {
            var stores = new StoreName[] { StoreName.Root, StoreName.AuthRoot, StoreName.CertificateAuthority, StoreName.My };
            foreach (StoreName storeName in stores) {
                using (var store = new X509Store(storeName, StoreLocation.LocalMachine)) {
                    try {
                        store.Open(OpenFlags.OpenExistingOnly);
                    } catch(CryptographicException) {
                        // Not all stores may be present
                        continue;
                    }

                    try {
                        var collection = store.Certificates.Cast<X509Certificate2>();
                        var cert = collection.FirstOrDefault(c => c.FriendlyName.EqualsIgnoreCase(name));
                        if (cert == null) {
                            cert = collection.FirstOrDefault(c => c.Subject.EqualsIgnoreCase(name));
                            if (cert != null) {
                                return cert;
                            }
                        }
                    } finally {
                        store.Close();
                    }
                }
            }
            return null;
        }
    }
}
