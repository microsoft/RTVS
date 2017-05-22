// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Protocol;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Host.Client {
    partial class RHost {
        private class CreateBlobRequest : Request<ulong> {
            private CreateBlobRequest(RHost host, Message message, CancellationToken cancellationToken)
                : base(host, message, cancellationToken) {
            }

            public static async Task<CreateBlobRequest> CreateAsync(RHost host, CancellationToken cancellationToken) {
                var message = host.CreateRequestMessage("?CreateBlob", new JArray());
                var request = new CreateBlobRequest(host, message, cancellationToken);

                await host.SendAsync(message, cancellationToken);
                return request;
            }

            public override void Handle(RHost host, Message response) {
                ulong id = response.GetUInt64(0, "blob_id");
                CompletionSource.TrySetResult(id);
            }
        }
    }
}
