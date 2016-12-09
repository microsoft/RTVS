// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.BrokerServices {
    public class ProfileWebService : WebService, IProfileWebService {
        private static readonly Uri Uri = new Uri("/profile", UriKind.Relative);

        public ProfileWebService(HttpClient httpClient, ICredentialsDecorator credentialsDecorator) 
            : base(httpClient, credentialsDecorator) {
        }

        public Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken)) =>
            HttpDeleteAsync(Uri, cancellationToken);
    }
}
