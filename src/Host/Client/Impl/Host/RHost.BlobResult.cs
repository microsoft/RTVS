// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    partial class RHost {
        private class BlobResult {
            public readonly BlobRequestKind Kind;

            public BlobResult(BlobRequestKind kind) {
                Kind = kind;
            }
        }
    }
}
