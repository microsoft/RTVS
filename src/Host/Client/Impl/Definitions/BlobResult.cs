// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    public class BlobResult {
        public readonly BlobRequestKind Kind;

        public BlobResult(BlobRequestKind kind) {
            Kind = kind;
        }
    }

    public static class BlobResultExtensions {
        public static SendBlobResult ToSendBlobResult(this BlobResult res) {
            return res as SendBlobResult;
        }

        public static GetBlobResult ToGetBlobResult(this BlobResult res) {
            return res as GetBlobResult;
        }
    }
}
