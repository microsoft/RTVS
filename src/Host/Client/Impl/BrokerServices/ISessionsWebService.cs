// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Client.BrokerServices {
    public interface ISessionsWebService {
        Task<IEnumerable<SessionInfo>> GetAsync();

        Task<SessionInfo> PutAsync(string id, SessionCreateRequest request);
    }

    public interface IRemoteUriWebService {
        Task<RemoteUriResponse> PostAsync(RemoteUriRequest request);
    }

}
