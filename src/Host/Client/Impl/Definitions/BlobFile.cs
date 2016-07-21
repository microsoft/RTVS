// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    public class BlobFile : BlobData, IRBlobFile {
        private readonly string _fileName;

        public BlobFile(long id, byte[] data, string fileName): base(id, data, RBlobKind.File) {
            _fileName = fileName;
        }

        public string RemoteFileName {
            get {
                return _fileName;
            }
        }
    }
}
