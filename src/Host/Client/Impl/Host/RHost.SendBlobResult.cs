// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    partial class RHost {
        private class SendBlobResult : BlobResult {
            public readonly long BlobId;

            public SendBlobResult(long blobId) : base(BlobRequestKind.Create) {
                BlobId = blobId;
            }
        }
    }
}
