// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Protocol;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Host.Client {
    partial class RHost {
        private class BlobReadRequest : Request<byte[]> {
            private BlobReadRequest(RHost host, Message message, CancellationToken cancellationToken)
                : base(host, message, cancellationToken) {
            }

            public static Task<BlobReadRequest> ReadAllAsync(RHost host, ulong blobId, CancellationToken cancellationToken) {
                return ReadAsync(host, blobId, 0, -1, cancellationToken);
            }

            public static async Task<BlobReadRequest> ReadAsync(RHost host, ulong blobId, long position, long count, CancellationToken cancellationToken) {
                var message = host.CreateRequestMessage("?ReadBlob", new JArray { blobId, position, count });
                var request = new BlobReadRequest(host, message, cancellationToken);
                await host.SendAsync(message, cancellationToken);
                return request;
            }

            public override void Handle(RHost host, Message response) => CompletionSource.TrySetResult(response.Blob);
        }
    }
}
