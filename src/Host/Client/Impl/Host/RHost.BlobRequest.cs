// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Host.Client {
    partial class RHost {
        private class BlobRequest : BaseRequest {
            public const string CreateBlobRequestMessageName = "?CreateBlob";
            public const string GetBlobRequestMessageName = "?GetBlob";
            public const string DestroyBlobNotifyMessageName = "!DestroyBlob";

            public const string CreateBlobResponseMessageName = ":CreateBlob";
            public const string GetBlobResponseMessageName = ":GetBlob";

            public readonly TaskCompletionSource<BlobResult> CompletionSource = new TaskCompletionSource<BlobResult>();
            public readonly BlobRequestKind Kind;

            private BlobRequest(string id, BlobRequestKind kind, string messageName) : base(id, messageName) {
                Kind = kind;
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
