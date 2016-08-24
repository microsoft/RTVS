// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Client.BrokerServices {
    public class SessionsWebService : WebService, ISessionsWebService {
        public SessionsWebService(HttpClient httpClient)
            : base(httpClient) {
        }

        private static readonly Uri getUri = new Uri("/sessions", UriKind.Relative);

        public Task<IEnumerable<SessionInfo>> GetAsync() =>
            HttpGetAsync<IEnumerable<SessionInfo>>(getUri);

        private static readonly UriTemplate putUri = new UriTemplate("/sessions/{name}");

        public Task<SessionInfo> PutAsync(string id, SessionCreateRequest request) =>
            HttpPutAsync<SessionCreateRequest, SessionInfo>(putUri, request, id);
    }

    public class RemoteUriWebService : WebService, IRemoteUriWebService {

        public RemoteUriWebService(HttpClient httpClient) : base(httpClient) {
        }

        private static readonly Uri postUri = new Uri("/remoteuri", UriKind.Relative);

        public Task<RemoteUriResponse> PostAsync(RemoteUriRequest request) =>
            HttpPostAsync<RemoteUriRequest, RemoteUriResponse>(postUri, request);
    }
}
