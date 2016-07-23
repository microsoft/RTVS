// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRBlobService {
        Task<ulong> CreateBlobAsync(byte[] data, CancellationToken cancellationToken = default(CancellationToken));
        Task<byte[]> GetBlobAsync(ulong blobId, CancellationToken cancellationToken = default(CancellationToken));
        Task DestroyBlobAsync(ulong[] blobIds, CancellationToken cancellationToken = default(CancellationToken));
    }
}
