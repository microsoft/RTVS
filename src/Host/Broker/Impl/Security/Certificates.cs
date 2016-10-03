// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Broker.Security {
    internal static class Certificates {
        public static X509Certificate2 GetTLSCertificate() {
            return FindCertificate((store) => store.Certificates.Find(
                    X509FindType.FindBySubjectName, "R Remote Services", validOnly: true));
        }

        private static X509Certificate2 FindCertificate(Func<X509Store, X509Certificate2Collection> search) {
            X509Certificate2 certificate = null;

            using (var store = new X509Store(StoreLocation.LocalMachine)) {
                store.Open(OpenFlags.OpenExistingOnly);
                try {
                    var cers = search(store);
                    certificate = cers.Count > 0 ? cers[0] : null;
                } finally {
                    store.Close();
                }
            }
            return certificate;
        }
    }
}
