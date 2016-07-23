// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Host.Client {
    partial class RHost {
        private class GetBlobRequest : Request<byte[]> {
            private GetBlobRequest(RHost host, Message message, CancellationToken cancellationToken)
                : base(host, message, cancellationToken) {
            }

            public static async Task<GetBlobRequest> SendAsync(RHost host, ulong blobId, CancellationToken cancellationToken) {
                var message = host.CreateMessage("?GetBlob", ulong.MaxValue, new JArray { blobId });
                var request = new GetBlobRequest(host, message, cancellationToken);
                await host.SendAsync(message, cancellationToken);
                return request;
            }

            public override void Handle(RHost host, Message response) {
                CompletionSource.TrySetResult(response.Blob);
            }
        }
    }
}
