// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Host.Client {
    partial class RHost {
        
        private class BlobRequest {
            public static readonly string CreateBlobRequestMessageName = "?CreateBlob";
            public static readonly string GetBlobRequestMessageName = "?GetBlob";
            public static readonly string DestroyBlobNotifyMessageName = "!DestroyBlob";

            public static readonly string CreateBlobResponseMessageName = ":CreateBlob";
            public static readonly string GetBlobResponseMessageName = ":GetBlob";

            public readonly string Id;
            public readonly string MessageName;
            public readonly TaskCompletionSource<BlobResult> CompletionSource = new TaskCompletionSource<BlobResult>();
            public readonly BlobRequestKind Kind;

            private BlobRequest(string id, BlobRequestKind kind, string messasgeName) {
                Id = id;
                Kind = kind;
                MessageName = messasgeName;
            }

            public static BlobRequest MakeCreateBlobRequest(RHost host, out JArray message, int blobCount) {
                string id;
                string messageName = BlobRequest.CreateBlobRequestMessageName;
                message = host.CreateMessage(host.CreateMessageHeader(out id, messageName, null, blobCount), string.Empty);
                return new BlobRequest(id, BlobRequestKind.Create, messageName);
            }

            public static BlobRequest MakeGetBlobsRequest(RHost host, out JArray message, params long[] blobIds) {
                string id;
                string messageName = BlobRequest.GetBlobRequestMessageName;
                message = host.CreateMessage(host.CreateMessageHeader(out id, messageName, null), blobIds);
                return new BlobRequest(id, BlobRequestKind.Get, messageName);
            }

            public static BlobRequest MakeDestroyBlobsRequest(RHost host, out JArray message, params long[] blobIds) {
                string id;
                string messageName = BlobRequest.DestroyBlobNotifyMessageName;
                message = host.CreateMessage(host.CreateMessageHeader(out id, messageName, null), blobIds);
                return new BlobRequest(id, BlobRequestKind.Destroy, messageName);
            }
        }
    }
}
