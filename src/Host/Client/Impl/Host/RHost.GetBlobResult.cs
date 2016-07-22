// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Host.Client {
    partial class RHost {
        private class GetBlobResult : BlobResult {
            public IReadOnlyList<IRBlob> Blobs;

            public GetBlobResult(List<IRBlob> blobs) : base(BlobRequestKind.Get) {
                Blobs = blobs ?? new List<IRBlob>();
            }
        }
    }
}
