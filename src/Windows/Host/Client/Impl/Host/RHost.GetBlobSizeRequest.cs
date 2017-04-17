// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Protocol;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Host.Client {
    partial class RHost {
        private class BlobSizeRequest : Request<long> {
            private BlobSizeRequest(RHost host, Message message, CancellationToken cancellationToken)
                : base(host, message, cancellationToken) {
            }

            public static async Task<BlobSizeRequest> GetSizeAsync(RHost host, ulong blobId, CancellationToken cancellationToken) {
                var message = host.CreateRequestMessage("?GetBlobSize", new JArray { blobId });
                var request = new BlobSizeRequest(host, message, cancellationToken);
                await host.SendAsync(message, cancellationToken);
                return request;
            }

            public static async Task<BlobSizeRequest> SetSizeAsync(RHost host, ulong blobId, long size, CancellationToken cancellationToken) {
                var message = host.CreateRequestMessage("?SetBlobSize", new JArray { blobId, size });
                var request = new BlobSizeRequest(host, message, cancellationToken);
                await host.SendAsync(message, cancellationToken);
                return request;
            }

            public override void Handle(RHost host, Message response) {
                long size = response.GetInt64(0, "blob_size");
                CompletionSource.TrySetResult(size);
            }
        }
    }
}
