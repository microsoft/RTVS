// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.BrokerServices {
    public interface IRemoteUriWebService {
        Task GetResponseAsync(HttpListenerContext context, string localBaseUrl, string remoteBaseUrl, CancellationToken ct);
    }
}
