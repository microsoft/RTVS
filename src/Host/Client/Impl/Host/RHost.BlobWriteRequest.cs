// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Protocol;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Host.Client {
    partial class RHost {
        private class BlobWriteRequest : Request<long> {
            private BlobWriteRequest(RHost host, Message message, CancellationToken cancellationToken)
                : base(host, message, cancellationToken) {
            }

            /// <summary>
            /// Appends data to the end of the blob
            /// </summary>
            public static async Task<BlobWriteRequest> WriteAsync(RHost host, ulong blobId, byte[] data, long position, CancellationToken cancellationToken) {
                var message = host.CreateRequestMessage("?WriteBlob", new JArray { blobId, position }, data);
                var request = new BlobWriteRequest(host, message, cancellationToken);
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
