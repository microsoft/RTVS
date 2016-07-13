// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRBlobHelper {
        Task<SendBlobResult> SendBlobAsync(byte[] data, CancellationToken cancellationToken = default(CancellationToken));
        Task<GetBlobResult> GetBlobAsync(long[] blobIds, CancellationToken cancellationToken = default(CancellationToken));
        Task DestroyBlobAsync(long[] blobIds, CancellationToken cancellationToken = default(CancellationToken));
    }
}
