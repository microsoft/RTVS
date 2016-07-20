// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    public class Blob {
        public readonly long BlobId;
        public readonly byte[] Data;

        public Blob(long blobId, byte[] data) {
            BlobId = blobId;
            Data = data;
        }
    }
}
