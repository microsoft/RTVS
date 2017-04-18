// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Logging;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Client.BrokerServices {
    public class SessionsWebService : WebService, ISessionsWebService {
        private static readonly Uri GetUri = new Uri("/sessions", UriKind.Relative);
        private Uri GetSessionUri(string name) {
            return new Uri($"{HttpClient.BaseAddress}sessions/{name}");
        }

        public SessionsWebService(HttpClient httpClient, ICredentialsDecorator credentialsDecorator, IActionLog log)
            : base(httpClient, credentialsDecorator, log) { }

        public Task<IEnumerable<SessionInfo>> GetAsync(CancellationToken cancellationToken = default(CancellationToken)) =>
            HttpGetAsync<IEnumerable<SessionInfo>>(GetUri, cancellationToken);

        public Task<SessionInfo> PutAsync(string id, SessionCreateRequest request, CancellationToken cancellationToken = default(CancellationToken)) =>
            HttpPutAsync<SessionCreateRequest, SessionInfo>(GetSessionUri(id), request, cancellationToken);

        public Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken)) =>
            HttpDeleteAsync(GetSessionUri(id), cancellationToken);
    }
}
