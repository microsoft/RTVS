// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Logging;

namespace Microsoft.R.Host.Client.BrokerServices {
    public class ProfileWebService : WebService, IProfileWebService {
        private static readonly Uri Uri = new Uri("/profile", UriKind.Relative);

        public ProfileWebService(HttpClient httpClient, ICredentialsDecorator credentialsDecorator, IActionLog log) 
            : base(httpClient, credentialsDecorator, log) {
        }

        public Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken)) =>
            HttpDeleteAsync(Uri, cancellationToken);
    }
}
