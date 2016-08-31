// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

namespace Microsoft.R.Host.Client.BrokerServices {
    public class RemoteUriWebService : WebService, IRemoteUriWebService {
        public RemoteUriWebService(HttpClient httpClient) : base(httpClient) {
        }

        private static readonly Uri postUri = new Uri("/remoteuri", UriKind.Relative);

        public Task<Stream> PostAsync(Stream request) => HttpPostAsync(postUri, request);
    }
}
