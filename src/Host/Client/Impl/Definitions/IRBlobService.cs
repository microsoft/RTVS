// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRBlobService {
        Task<long> SendBlobAsync(byte[] data, CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyList<IRBlobData>> GetBlobAsync(IEnumerable<long> blobIds, CancellationToken cancellationToken = default(CancellationToken));
        Task DestroyBlobAsync(IEnumerable<long> blobIds, CancellationToken cancellationToken = default(CancellationToken));
    }
}
