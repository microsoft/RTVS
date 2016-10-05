// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Client.BrokerServices {
    public interface ISessionsWebService {
        Task<IEnumerable<SessionInfo>> GetAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<SessionInfo> PutAsync(string id, SessionCreateRequest request, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken));
    }
}
